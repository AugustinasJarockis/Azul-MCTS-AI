using AzulBoardGame.GameComponentInterfaces;
using AzulBoardGame.Enums;
using AzulBoardGame.GameState;
using AzulBoardGame.Players.PlayerBase;

namespace AzulBoardGame.Players
{
    public class HeuristicAI : IPlayerAI
    {
        private List<PlateState> platesWithCount(List<PlateState> plates, TileType type, int count) => [.. plates.Where(p => p.TileTypes.Count(t => t == type) == count)];
        private int tilesInCenter(TilePlatesState tilePlates, TileType type) => tilePlates.CenterTileTypes.Count(t => t == type);

        private bool CanReasonablyFit(PlayerBoardState playerState, TileType type, int count) {
            for (int rowNr = Math.Max(0, count - 1); rowNr < 5; rowNr++) {
                if (CanReasonablyFitIntoRow(playerState, type, count, rowNr >= 2 ? 1 : 0, rowNr))
                    return true;
            }
            return false;
        }

        private bool CanReasonablyFitIntoRow(TileType type, int count, int degreeOfFreedom, ITileRow row) => 
            (row.RowTileType == type || row.IsEmpty) 
            && row.FreeSlotCount >= count - degreeOfFreedom;
        private bool CanReasonablyFitIntoRow(PlayerBoardState playerState, TileType type, int count, int degreeOfFreedom, int rowNr) => 
            (playerState.tileRows[rowNr].RowTileType == type || playerState.tileRows[rowNr].IsEmpty) 
            && playerState.tileRows[rowNr].FreeSlotCount >= count - degreeOfFreedom
            && !playerState.tileGrid.RowHasType(rowNr, type);
        private bool CanFitIntoRow(TileType type, int count, ITileRow row) => 
            (row.RowTileType == type || row.IsEmpty) 
            && row.FreeSlotCount >= count;

        private (byte, TileType, int) SelectTiles(GeneralGameState gameState) {
            TilePlatesState tilePlates = gameState.TilePlatesState;
            PlayerBoardState playerBoard = gameState.PlayerBoardStates[gameState.CurrentPlayer];

            int centerTilesExist = tilePlates.CenterTileCount != 0 ? 1 : 0;

            var plates = tilePlates.Plates.Where(p => !p.IsEmpty).ToList();

            for (int type = 1; type < 6; type++) {
                int tileInCenterCount = tilesInCenter(tilePlates, (TileType)type);
                if (tileInCenterCount >= 5 && CanReasonablyFit(playerBoard, (TileType)type, tileInCenterCount)) {
                    return (0, (TileType)type, tileInCenterCount);
                }
            }

            for (int count = 4; count > 0; count--) {
                for (int type = 1; type < 6; type++) {
                    var platesToSelect = platesWithCount(tilePlates.Plates, (TileType)type, count);
                    if (platesToSelect.Count != 0 && CanReasonablyFit(playerBoard, (TileType)type, count)) {
                        return ((byte)(tilePlates.Plates.IndexOf(platesToSelect[0]) + 1), (TileType)type, count);
                    }

                    if (tilesInCenter(tilePlates, (TileType)type) == count && CanReasonablyFit(playerBoard, (TileType)type, count)) {
                        return (0, (TileType)type, count);
                    }
                }
            }

            // Pick the lowest possible
            for (int count = 1; count < 5; count++) {
                for (int type = 1; type < 6; type++) {
                    var platesToSelect = platesWithCount(plates, (TileType)type, count);
                    if (platesToSelect.Count != 0)
                        return ((byte)(tilePlates.Plates.IndexOf(platesToSelect[0]) + 1), (TileType)type, count);

                    if (tilesInCenter(tilePlates, (TileType)type) == count)
                        return (0, (TileType)type, count);
                }
            }

            //Select first
            TileType finalType = tilePlates.CenterTileTypes[0];
            int finalCount = tilesInCenter(tilePlates, finalType);
            return (0, finalType, finalCount);
        }

        public byte SelectRow(PlayerBoardState playerState, TileType selectedType, int selectedTileCount) {
            
            List<(TileRowState row, int rowNr)> possibleRows = [];

            for (int rowNr = 0; rowNr < playerState.tileRows.Count; rowNr++) {
                if (!playerState.tileRows[rowNr].IsFull
                    && (playerState.tileRows[rowNr].RowTileType == null || playerState.tileRows[rowNr].RowTileType == selectedType)
                    && !playerState.tileGrid.RowHasType(rowNr, selectedType))

                    possibleRows.Add((playerState.tileRows[rowNr], rowNr));
            }

            if (possibleRows.Count > 0) {

                for (int rowNr = 0; rowNr < possibleRows.Count; rowNr++) {
                    if (CanFitIntoRow(selectedType, selectedTileCount, possibleRows[rowNr].row)) {
                        return (byte)possibleRows[rowNr].rowNr;
                    }
                }

                for (int rowNr = 0; rowNr < possibleRows.Count; rowNr++) {
                    if (CanReasonablyFitIntoRow(selectedType, selectedTileCount, 1, possibleRows[rowNr].row)) {
                        return (byte)possibleRows[rowNr].rowNr;
                    }
                }

                return (byte)possibleRows.MaxBy(row => row.row.FreeSlotCount)!.rowNr; 
            }
            else {
                return 5;
            }
        }

        public (byte, TileType, byte) ChooseMove(GeneralGameState gameState) {
            (byte plate, TileType type, byte row) moveToMake;
            (moveToMake.plate, moveToMake.type, int selectedTileCount) = SelectTiles(gameState);
            moveToMake.row = SelectRow(gameState.PlayerBoardStates[gameState.CurrentPlayer], moveToMake.type, selectedTileCount);            
            return moveToMake;
        }
    }
}
