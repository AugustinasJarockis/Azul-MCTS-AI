using AzulBoardGame.Enums;

namespace AzulBoardGame.GameState
{
    public class ProcessingLineState
    {
        private List<TileType> _processedTiles = [];

        public ProcessingLineState() {}
        public ProcessingLineState(List<TileType> processedTiles) {
            this._processedTiles = processedTiles;
        }
        public ProcessingLineState Copy() {
            return new ProcessingLineState([.._processedTiles]);
        }

        public void Reset() => _processedTiles.Clear();
        
        public int GetListState() {
            //List<int> stateList = [];

            //for (int i = 0; i < 7; i++) {
            //    if (_processedTiles.Count > i) {
            //        if (_processedTiles[i] == TileType.First) {
            //            stateList.Add(6);
            //            continue;
            //        }
            //        stateList.Add((int)_processedTiles[i]);
            //    }
            //    else
            //        stateList.Add(0);
            //}

            return GetPointLoss();
        }
        public void AddTile(TileType tile, ITileBank tileBank) {
            if (_processedTiles.Count < 7) {
                _processedTiles.Add(tile);
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

        public int GetPointLoss() => _processedTiles.Count switch {
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

            foreach (var tile in _processedTiles) {
                if (tile != TileType.First)
                    tileBank.DiscardTiles(tile);
            }

            _processedTiles.Clear();
            return pointLoss;
        }
    }
}
