using AzulBoardGame.Enums;
using AzulBoardGame.GamePlates;
using AzulBoardGame.GameTilePlates;
using AzulBoardGame.PlayerBoard;
using AzulBoardGame.PlayerBoard.PlayerTileGrid;
using AzulBoardGame.PlayerBoard.PlayerTileRow;
using AzulBoardGame.PlayerBoard.PointCounter;
using AzulBoardGame.Players.PlayerBase;
namespace AzulBoardGame.Players.MCTS
{
    internal class HeuristicStateDelver : PlayerState<HeuristicStateDelver>
    {
        public HeuristicStateDelver(
            TilePlatesState tilePlates,
            ITileBank tileBank
            )
            : base(tilePlates, tileBank) { }
        public HeuristicStateDelver(
            TilePlatesState tilePlates,
            ITileBank tileBank,
            ProcessingLineState processingLine,
            InvisiblePointCounter pointCounter,
            TileGridState tileGrid,
            List<TileRowState> tileRows
            )
            : base(tilePlates, tileBank, processingLine, pointCounter, tileGrid, tileRows) { }

        public override HeuristicStateDelver Copy(TilePlatesState tilePlates, ITileBank tileBank) {
            return new HeuristicStateDelver(tilePlates, tileBank, processingLine, pointCounter, tileGrid, tileRows);
        }

        private List<(IPlate plate, byte plateNr)> platesWithCount(List<IPlate> plates, TileType type, int count) {
            List<(IPlate plate, byte plateNr)> selectedPlates = [];
            for (byte i = 0; i < plates.Count; i++) {
                if (plates[i].TileTypes.Count(t => t == type) == count) {
                    selectedPlates.Add((plates[i], (byte)_tilePlates.Plates.IndexOf(plates[i])));
                }
            }
            return selectedPlates;
        }
        private int tilesInCenter(TileType type) => _tilePlates.CenterTileTypes.Count(t => t == type);

        private bool CanReasonablyFit(TileType type, int count) {
            for (int rowNr = Math.Max(0, count - 1); rowNr < 5; rowNr++) {
                if (CanReasonablyFitIntoRow(type, count, rowNr >= 2 ? 1 : 0, rowNr))
                    return true;
            }
            return false;
        }

        private bool CanReasonablyFitIntoRow(TileType type, int count, int degreeOfFreedom, ITileRow row) => 
            (row.RowTileType == type || row.IsEmpty) 
            && row.FreeSlotCount >= count - degreeOfFreedom;
        private bool CanReasonablyFitIntoRow(TileType type, int count, int degreeOfFreedom, int rowNr) =>
            (tileRows[rowNr].RowTileType == type || tileRows[rowNr].IsEmpty)
            && tileRows[rowNr].FreeSlotCount >= count - degreeOfFreedom
            && !tileGrid.RowHasType(rowNr, type);
        private bool CanFitIntoRow(TileType type, int count, ITileRow row) => (row.RowTileType == type || row.IsEmpty) && row.FreeSlotCount >= count;

        public override (byte, TileType, byte) SelectTiles() {
            _tilePlates.SetSelectionCallback(ManageSelectedTiles);

            int centerTilesExist = _tilePlates.CenterTileCount != 0 ? 1 : 0;

            var plates = _tilePlates.Plates.Where(p => !p.IsEmpty).ToList();

            for (int type = 1; type < 6; type++) {
                int tileInCenterCount = tilesInCenter((TileType)type);
                if (tileInCenterCount >= 5 && CanReasonablyFit((TileType)type, tileInCenterCount)) {
                    _tilePlates.SelectTiles((TileType)type);
                    var chosenRow = (byte)selectedRow!;
                    selectedRow = null;
                    return (0, (TileType)type, chosenRow);
                }
            }

            for (int count = 4; count > 0; count--) {
                for (int type = 1; type < 6; type++) {
                    var platesToSelect = platesWithCount(plates, (TileType)type, count);
                    if (platesToSelect.Count != 0 && CanReasonablyFit((TileType)type, count)) {
                        platesToSelect[0].plate.SelectTiles((TileType)type);
                        var chosenRow = (byte)selectedRow!;
                        selectedRow = null;
                        return ((byte)(platesToSelect[0].plateNr + 1), (TileType)type, chosenRow);
                    }

                    if (tilesInCenter((TileType)type) == count && CanReasonablyFit((TileType)type, count)) {
                        _tilePlates.SelectTiles((TileType)type);
                        var chosenRow = (byte)selectedRow!;
                        selectedRow = null;
                        return (0, (TileType)type, chosenRow);
                    }
                }
            }

            // Pick the lowest possible
            for (int count = 1; count < 5; count++) {
                for (int type = 1; type < 6; type++) {
                    var platesToSelect = platesWithCount(plates, (TileType)type, count);
                    if (platesToSelect.Count != 0) {
                        platesToSelect[0].plate.SelectTiles((TileType)type);
                        var chosenRow = (byte)selectedRow!;
                        selectedRow = null;
                        return ((byte)(platesToSelect[0].plateNr + 1), (TileType)type, chosenRow);
                    }

                    if (tilesInCenter((TileType)type) == count) {
                        _tilePlates.SelectTiles((TileType)type);
                        var chosenRow = (byte)selectedRow!;
                        selectedRow = null;
                        return (0, (TileType)type, chosenRow);
                    }
                }
            }

            //Select first
            var typeOfFirst = _tilePlates.CenterTileTypes[0];
            _tilePlates.SelectTiles(typeOfFirst);
            var chosenRowForFirst = (byte)selectedRow!;
            selectedRow = null;
            return (0, typeOfFirst, chosenRowForFirst);
        }

        public override void SelectRow() {
            List<(byte rowNr, TileRowState state)> possibleRows = [];

            for (byte rowNr = 0; rowNr < tileRows.Count; rowNr++) {
                if (!tileRows[rowNr].IsFull
                    && (tileRows[rowNr].RowTileType == null || tileRows[rowNr].RowTileType == selectedTiles[0])
                    && !tileGrid.RowHasType(rowNr, selectedTiles[0]))

                    possibleRows.Add((rowNr, tileRows[rowNr]));
            }

            if (possibleRows.Count > 0) {

                for (int rowNr = 0; rowNr < possibleRows.Count; rowNr++) {
                    if (CanFitIntoRow(selectedTiles[0], selectedTiles.Count, possibleRows[rowNr].state)) {
                        TakeSelectedTiles(possibleRows[rowNr].state);
                        selectedRow = possibleRows[rowNr].rowNr;
                        return;
                    }
                }

                for (int rowNr = 0; rowNr < possibleRows.Count; rowNr++) {
                    if (CanReasonablyFitIntoRow(selectedTiles[0], selectedTiles.Count, 1, possibleRows[rowNr].state)) {
                        TakeSelectedTiles(possibleRows[rowNr].state);
                        selectedRow = possibleRows[rowNr].rowNr;
                        return;
                    }
                }

                var maxRow = possibleRows.MaxBy(row => row.state.FreeSlotCount);
                TakeSelectedTiles(maxRow.state!);
                selectedRow = maxRow.rowNr;
            }
            else {
                DiscardSelectedTiles();
                selectedRow = 5;
            }
        }

        protected override void RemoveSelectedTiles() => selectedTiles.Clear();
    }
}
