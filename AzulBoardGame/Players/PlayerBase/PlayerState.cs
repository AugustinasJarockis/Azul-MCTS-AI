using AzulBoardGame.Enums;
using AzulBoardGame.GameTilePlates;
using AzulBoardGame.PlayerBoard;
using AzulBoardGame.PlayerBoard.PlayerTileGrid;
using AzulBoardGame.PlayerBoard.PlayerTileRow;
using AzulBoardGame.PlayerBoard.PointCounter;

namespace AzulBoardGame.Players.PlayerBase
{
    internal abstract class PlayerState<T>
    {
        static private Random rnd = new(DateTime.Now.Microsecond * DateTime.Now.Millisecond);
        protected readonly TilePlatesState _tilePlates;
        private readonly ITileBank _tileBank;

        protected ProcessingLineState processingLine;
        protected InvisiblePointCounter pointCounter;
        protected List<TileRowState> tileRows = [];
        protected List<TileType> selectedTiles = [];
        protected TileGridState tileGrid;

        public int Points => pointCounter.Points;

        protected byte? selectedRow = null;

        public PlayerState(
            TilePlatesState tilePlates,
            ITileBank tileBank
            ) {
            _tilePlates = tilePlates;
            _tileBank = tileBank;

            processingLine = new(_tileBank);
            pointCounter = new InvisiblePointCounter();
            tileGrid = new();

            for (int i = 0; i < 5; i++) {
                tileRows.Add(new(i + 1, processingLine, tileBank));
            }
        }

        public PlayerState(
            TilePlatesState tilePlates,
            ITileBank tileBank,
            ProcessingLineState processingLine,
            InvisiblePointCounter pointCounter,
            TileGridState tileGrid,
            List<TileRowState> tileRows
            ) {
            _tilePlates = tilePlates;
            _tileBank = tileBank;

            this.processingLine = processingLine.Copy(_tileBank);
            this.pointCounter = pointCounter.Copy();
            this.tileGrid = tileGrid.Copy();

            for (int i = 0; i < 5; i++) {
                this.tileRows.Add(tileRows[i].Copy(this.processingLine, _tileBank));
            }
        }

        public abstract T Copy(TilePlatesState tilePlates, ITileBank tileBank);

        private bool CanBePlacedIntoRow(TileType type, int row) => 
            (tileRows[row].RowTileType == type || tileRows[row].IsEmpty) 
            && !tileRows[row].IsFull 
            && !tileGrid.RowHasType(row, type);

        public List<(byte, TileType, byte)> GetPossibleMoves() {
            List<(byte, TileType, byte)> moves = [];

            List<TileType> centerTypes = [.. _tilePlates.CenterTileTypes.Distinct()];
            foreach (var centerType in centerTypes) {
                for (byte i = 0; i < 5; i++) {
                    if (CanBePlacedIntoRow(centerType, i))
                        moves.Add((0, centerType, i));
                }
                moves.Add((0, centerType, 5));
            }

            for (byte i = 0; i < _tilePlates.Plates.Count; i++) {
                List<TileType> plateTypes = [.. _tilePlates.Plates[i].TileTypes.Distinct()];
                foreach (var tileType in plateTypes) {
                    for (byte i2 = 0; i2 < 5; i2++) {
                        if (CanBePlacedIntoRow(tileType, i2))
                            moves.Add(((byte)(i + 1), tileType, i2));
                    }
                    moves.Add(((byte)(i + 1), tileType, 5));
                }
            }

            return [.. moves.OrderBy(m => rnd.Next())];
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

            pointCounter.UpdatePoints(totalPointChange);
        }

        public bool HasFinished() {
            for (int i = 0; i < 5; i++)
                if (tileGrid.RowIsFull(i))
                    return true;

            return false;
        }

        public void CompleteRound() {
            for (int i = 0; i < tileRows.Count; i++) {
                if (tileRows[i].IsFull) {
                    TileType tileToTransfer = tileRows[i].PrepareForTileTransfer();
                    int pointsGained = tileGrid.AddTile(i, tileToTransfer);
                    pointCounter.UpdatePoints(pointsGained);
                }
            }
            int pointsLost = processingLine.Clear();
            pointCounter.UpdatePoints(-pointsLost);
        }

        public abstract (byte, TileType, byte) SelectTiles();

        public void SelectTiles((byte plate, TileType type, byte row) move) {
            _tilePlates.SetSelectionCallback(ManageSelectedTiles);
            selectedRow = move.row;

            if (move.plate == 0) {
                _tilePlates.SelectTiles(move.type);
            }
            else {
                _tilePlates.Plates[move.plate - 1].SelectTiles(move.type);
            }
        }
        public abstract void SelectRow();

        private void SelectRow(byte row) {
            if (row == 5)
                DiscardSelectedTiles();
            else
                TakeSelectedTiles(tileRows[row]);
            selectedRow = null;
        }

        public void ManageSelectedTiles(List<TileType> tiles) {
            if (tiles[^1] == TileType.First) {
                processingLine.AddTile(tiles[^1]);
                tiles.Remove(tiles[^1]);
            }

            selectedTiles = tiles;

            _tilePlates.ClearSelectionCallback();
            if (selectedRow != null)
                SelectRow((byte)selectedRow);
            else
                SelectRow();
        }

        public void TakeSelectedTiles(TileRowState tileRow) {
            tileRow.AddTiles(selectedTiles);
            RemoveSelectedTiles();
        }

        public void DiscardSelectedTiles() {
            processingLine.AddTiles(selectedTiles);
            RemoveSelectedTiles();
        }
        protected abstract void RemoveSelectedTiles();
    }
}
