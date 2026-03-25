using AzulBoardGame.Enums;
using System.Diagnostics.Contracts;

namespace AzulBoardGame.GameState
{
    internal class PlateState
    {
        private List<TileType> _tiles = [];
        public List<TileType> TileTypes => _tiles;
        public int TileCount => _tiles.Count;
        public bool IsEmpty => _tiles.Count == 0;

        public PlateState() {}
        public PlateState(List<TileType> tiles) {
            _tiles = tiles;
        }

        public void PlaceTiles(List<TileType> tileTypes) => _tiles.AddRange(tileTypes);
        public void Clear() => _tiles.Clear();
        public PlateState Copy() {
            return new([.._tiles]);
        }

        public (List<TileType>, List<TileType>) SelectTiles(TileType type) {
            List<TileType> selectedTiles = [.. _tiles.Where(t => t == type)];
            List<TileType> unselectedTiles = [.. _tiles.Where(t => t != type)];
            
            _tiles.Clear();

            return (selectedTiles, unselectedTiles);
        }

    }
}
