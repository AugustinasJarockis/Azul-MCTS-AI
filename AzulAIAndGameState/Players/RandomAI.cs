using AzulBoardGame.Enums;
using AzulBoardGame.GameState;
using AzulBoardGame.Players.PlayerBase;

namespace AzulBoardGame.Players
{
    public class RandomAI : IPlayerAI
    {
        private Random rnd = new(DateTime.Now.Microsecond * DateTime.Now.Millisecond);

        public (byte, TileType) SelectTiles(GeneralGameState gameState) {
            TilePlatesState tilePlates = gameState.TilePlatesState;
            PlayerBoardState playerBoard = gameState.PlayerBoardStates[gameState.CurrentPlayer];

            int centerTilesExist = tilePlates.CenterTileCount != 0 ? 1 : 0;

            var plates = tilePlates.Plates.Where(p => !p.IsEmpty).ToList();
            int selection = rnd.Next(plates.Count + centerTilesExist);

            if (selection == 0 && centerTilesExist != 0) {
                var centerTileTypes = tilePlates.CenterTileTypes;
                int tileToSelect = rnd.Next(tilePlates.CenterTileTypes.Count);
                var selectedType = centerTileTypes[tileToSelect];
                return (0, selectedType);
            }
            else {
                int tileToSelect = rnd.Next(4);
                var selectedType = plates[selection - centerTilesExist].TileTypes[tileToSelect];
                return ((byte)(tilePlates.Plates.IndexOf(plates[selection - centerTilesExist]) + 1), selectedType);
            }
        }

        public int SelectRow(PlayerBoardState playerState, TileType selectedType) {
            List<int> possibleRows = [];

            for (int i = 0; i < playerState.tileRows.Count; i++) {
                if (playerState.CanBePlacedIntoRow(selectedType, i))
                    possibleRows.Add(i);
            }

            if (possibleRows.Count > 0) {
                int rowToSelect = rnd.Next(possibleRows.Count);
                return possibleRows[rowToSelect];
            }
            else {
                return 5;
            }
        }
        public (byte, TileType, byte) ChooseMove(GeneralGameState gameState) {
            (byte plate, TileType type, byte row) moveToMake;
            (moveToMake.plate, moveToMake.type) = SelectTiles(gameState);
            moveToMake.row = (byte)SelectRow(gameState.PlayerBoardStates[gameState.CurrentPlayer], moveToMake.type);
            return moveToMake;
        }
    }
}
