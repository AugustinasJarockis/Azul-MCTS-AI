using AzulBoardGame.Enums;

namespace AzulBoardGame.GameState
{
    public class PlateState
    {
        private List<TileType> _tiles = [];
        public List<TileType> TileTypes => _tiles;
        public int TileCount => _tiles.Count;
        public bool IsEmpty => _tiles.Count == 0;

        public PlateState() {}
        public PlateState(List<TileType> tiles) {
            _tiles = tiles;
        }

        public PlateState(List<int> listState) : this(listState.Where(e => e != 0).Select(e => (TileType)e).ToList()) {}

        public void PlaceTiles(List<TileType> tileTypes) => _tiles.AddRange(tileTypes);
        public void Clear() => _tiles.Clear();
        public PlateState Copy() {
            return new([.._tiles]);
        }

        public List<int> GetListState() {
            List<int> stateList = [];
            for (int i = 0; i < 4; i++) {
                if (_tiles.Count > i)
                    stateList.Add((int)_tiles[i]);
                else
                    stateList.Add(0);
            }
            return stateList;
        }

        public (List<TileType>, List<TileType>) SelectTiles(TileType type) {
            List<TileType> selectedTiles = [.. _tiles.Where(t => t == type)];
            List<TileType> unselectedTiles = [.. _tiles.Where(t => t != type)];
            
            _tiles.Clear();

            return (selectedTiles, unselectedTiles);
        }

    }
}
