using AzulBoardGame.Enums;

namespace AzulBoardGame.GameState
{
    internal class ProcessingLineState
    {
        private List<TileType> processedTiles = [];

        public ProcessingLineState() {}
        public ProcessingLineState(List<TileType> processedTiles) {
            this.processedTiles = processedTiles;
        }
        public ProcessingLineState Copy() {
            return new ProcessingLineState([..processedTiles]);
        }

        public void Reset() => processedTiles.Clear();

        public void AddTile(TileType tile, ITileBank tileBank) {
            if (processedTiles.Count < 7) {
                processedTiles.Add(tile);
            }
            else {
                if (tile != TileType.First)
                    tileBank.DiscardTiles(tile);
            }
        }

        public void AddTiles(List<TileType> tiles, ITileBank tileBank) {
            foreach (var tile in tiles)
                AddTile(tile, tileBank);
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

        public int Clear(ITileBank tileBank) {
            int pointLoss = GetPointLoss();

            foreach (var tile in processedTiles) {
                if (tile != TileType.First)
                    tileBank.DiscardTiles(tile);
            }

            processedTiles.Clear();
            return pointLoss;
        }
    }
}
