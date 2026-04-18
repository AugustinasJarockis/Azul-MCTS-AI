using AzulBoardGame.Enums;
using AzulBoardGame.PlayerBoard.PlayerTileGrid;

namespace AzulBoardGame.GameState
{
    public class PlayerBoardState
    {
        public int Points { get; private set; } = 0;
        public List<TileRowState> tileRows = [];
        public List<TileType> selectedTiles = [];
        public ProcessingLineState processingLine;
        public TileGridState tileGrid;

        public (byte, TileType, byte)? MoveMade;

        public PlayerBoardState() {
            processingLine = new ProcessingLineState();
            tileGrid = new TileGridState();
            
            for (int i = 1; i <= 5; i++) {
                tileRows.Add(new(i));
            }
        }

        public PlayerBoardState(
            int points, 
            List<TileRowState> tileRows, 
            List<TileType> selectedTiles,
            ProcessingLineState processingLine,
            TileGridState tileGrid
        ) {
            Points = points;
            this.tileRows = tileRows;
            this.selectedTiles = selectedTiles;
            this.processingLine = processingLine;
            this.tileGrid = tileGrid;
        }

        public PlayerBoardState Copy() {
            List<TileRowState> tileRowsCopies = [..tileRows.Select(r => r.Copy())];
            return new(Points, tileRowsCopies, [..selectedTiles], processingLine.Copy(), tileGrid.Copy());
        }

        public void Reset() {
            Points = 0;
            foreach(TileRowState row in tileRows) {
                row.Reset();
            }
            selectedTiles.Clear();
            processingLine.Reset();
            tileGrid.Reset();
        }

        public (List<List<int>>, List<(int, int)>, List<int>, int) GetListState() 
            => (tileGrid.GetListState(), [.. tileRows.Select(r => r.GetListState())], processingLine.GetListState(), Points);

        public void UpdatePoints(int pointsChange) {
            Points += pointsChange;
            Points = Math.Max(Points, 0);
        }

        public bool CanBePlacedIntoRow(TileType type, int row) =>
            (tileRows[row].RowTileType == type || tileRows[row].IsEmpty)
            && !tileRows[row].IsFull
            && !tileGrid.RowHasType(row, type);

        public List<(byte, TileType, byte)> GetPossibleMoves(TilePlatesState tilePlates) {
            List<(byte, TileType, byte)> moves = [];

            List<TileType> centerTypes = [.. tilePlates.CenterTileTypes.Distinct()];
            foreach (var centerType in centerTypes) {
                for (byte i = 0; i < 5; i++) {
                    if (CanBePlacedIntoRow(centerType, i))
                        moves.Add((0, centerType, i));
                }
                moves.Add((0, centerType, 5));
            }

            for (byte i = 0; i < tilePlates.Plates.Count; i++) {
                List<TileType> plateTypes = [.. tilePlates.Plates[i].TileTypes.Distinct()];
                foreach (var tileType in plateTypes) {
                    for (byte i2 = 0; i2 < 5; i2++) {
                        if (CanBePlacedIntoRow(tileType, i2))
                            moves.Add(((byte)(i + 1), tileType, i2));
                    }
                    moves.Add(((byte)(i + 1), tileType, 5));
                }
            }

            return moves;
        }

        public void CalculateAdditionalPoints() {
            int totalPointChange = 0;
            for (int i = 0; i < 5; i++) {
                if (tileGrid.RowIsFull(i))
                    totalPointChange += 2;

                if (tileGrid.CollumnIsFull(i))
                    totalPointChange += 7;

                if (tileGrid.TypeIsComplete((TileType)(i + 1)))
                    totalPointChange += 10;
            }

            UpdatePoints(totalPointChange);
        }

        public bool HasFinished() {
            for (int i = 0; i < 5; i++)
                if (tileGrid.RowIsFull(i))
                    return true;

            return false;
        }

        public void CompleteRound(ITileBank tileBank) {
            for (int i = 0; i < tileRows.Count; i++) {
                if (tileRows[i].IsFull) {
                    TileType tileToTransfer = tileRows[i].PrepareForTileTransfer(tileBank);
                    int pointsGained = tileGrid.AddTile(i, tileToTransfer);
                    UpdatePoints(pointsGained);
                }
            }
            int pointsLost = processingLine.Clear(tileBank);
            UpdatePoints(-pointsLost);
        }

        public void SelectRow(byte row, ITileBank tileBank) {
            if (row == 5)
                DiscardSelectedTiles(tileBank);
            else
                TakeSelectedTiles(tileRows[row]);
        }

        public void ManageSelectedTiles(List<TileType> tiles, ITileBank tileBank) {
            if (tiles[^1] == TileType.First) {
                processingLine.AddTile(tiles[^1], tileBank);
                tiles.Remove(tiles[^1]);
            }

            selectedTiles = tiles;
        }

        private void TakeSelectedTiles(TileRowState tileRow) {
            tileRow.AddTiles(selectedTiles);
            RemoveSelectedTiles();
        }

        private void DiscardSelectedTiles(ITileBank tileBank) {
            processingLine.AddTiles(selectedTiles, tileBank);
            RemoveSelectedTiles();
        }
        protected void RemoveSelectedTiles() {
            selectedTiles.Clear();
        }
    }
}
