using AzulBoardGame.Enums;

namespace AzulBoardGame.PlayerBoard.PlayerTileRow
{
    internal class TileRowState : ITileRow
    {
        private readonly ProcessingLineState _processingLine;
        private readonly ITileBank _tileBank;
        private readonly int _capacity;

        private List<TileType> rowTiles = [];
        public TileType? RowTileType => rowTiles.Count != 0 ? rowTiles[0] : null;
        public bool IsFull => rowTiles.Count == _capacity;
        public bool IsEmpty => rowTiles.Count == 0;
        public int TileCount => rowTiles.Count;
        public int FreeSlotCount => _capacity - rowTiles.Count;
        public TileRowState(
            int capacity,
            ProcessingLineState processingLine,
            ITileBank tileBank
            ) {
            _processingLine = processingLine;
            _tileBank = tileBank;
            _capacity = capacity;
        }

        public TileRowState(
            int capacity,
            ProcessingLineState processingLine,
            ITileBank tileBank,
            List<TileType> rowTiles
            ) {
            _processingLine = processingLine;
            _tileBank = tileBank;
            _capacity = capacity;
            this.rowTiles = [..rowTiles];
        }

        public TileRowState Copy(ProcessingLineState processingLine, ITileBank tileBank) {
            return new (_capacity, processingLine, tileBank, rowTiles);
        }

        public void AddTiles(List<TileType> tiles) {
            while (rowTiles.Count < _capacity && tiles.Count > 0) {
                rowTiles.Add(tiles[0]);
                tiles.RemoveAt(0);
            }
            _processingLine.AddTiles(tiles);
        }

        public TileType PrepareForTileTransfer() {
            var firstTile = rowTiles[0];
            _tileBank.DiscardTiles(rowTiles[0], rowTiles.Count - 1);

            rowTiles.Clear();
            return firstTile;
        }
    }
}
