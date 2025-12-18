using AzulBoardGame.Enums;

namespace AzulBoardGame.PlayerBoard
{
    internal class ProcessingLineState
    {
        private readonly ITileBank _tileBank;
        private List<TileType> processedTiles = [];

        public ProcessingLineState(ITileBank tileBank) {
            _tileBank = tileBank;
        }
        public ProcessingLineState(ITileBank tileBank, List<TileType> processedTiles) {
            _tileBank = tileBank;
            this.processedTiles = [..processedTiles];
        }
        public ProcessingLineState Copy(ITileBank tileBank) {
            return new ProcessingLineState(tileBank, processedTiles);
        }

        public void AddTile(TileType tile) {
            if (processedTiles.Count < 7) {
                processedTiles.Add(tile);
            }
            else {
                if (tile != TileType.First)
                    _tileBank.DiscardTiles(tile);
            }
        }

        public void AddTiles(List<TileType> tiles) {
            foreach (var tile in tiles)
                AddTile(tile);
        }

        public int GetPointLoss() => processedTiles.Count switch {
            0 => 0,
            1 => 1,
            2 => 2,
            3 => 4,
            4 => 6,
            5 => 8,
            6 => 11,
            7 => 14,
            _ => 14
        };

        public int Clear() {
            int pointLoss = GetPointLoss();

            foreach (var tile in processedTiles) {
                if (tile != TileType.First)
                    _tileBank.DiscardTiles(tile);
            }

            processedTiles.Clear();
            return pointLoss;
        }
    }
}
