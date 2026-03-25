using AzulBoardGame.Enums;
using AzulBoardGame.PlayerBoard.PlayerTileRow;

namespace AzulBoardGame.GameState
{
    internal class TileRowState : ITileRow
    {
        public readonly int Capacity;

        private List<TileType> rowTiles = [];
        public TileType? RowTileType => rowTiles.Count != 0 ? rowTiles[0] : null;
        public bool IsFull => rowTiles.Count == Capacity;
        public bool IsEmpty => rowTiles.Count == 0;
        public int TileCount => rowTiles.Count;
        public int FreeSlotCount => Capacity - rowTiles.Count;

        public TileRowState(int capacity) {
            Capacity = capacity;
        }

        public TileRowState(int capacity, List<TileType> rowTilesCopy) {
            Capacity = capacity;
            rowTiles = rowTilesCopy;
        }

        public TileRowState Copy() {
            return new(Capacity, [.. rowTiles]);
        }

        public void Reset() => rowTiles.Clear();

        public List<TileType> AddTiles(List<TileType> tiles) {
            int tilesToTake = Math.Min(Capacity - rowTiles.Count, tiles.Count);
            rowTiles.AddRange(tiles.Take(tilesToTake));
            return [..tiles.Skip(tilesToTake)];
        }

        public TileType PrepareForTileTransfer() {
            var firstTile = rowTiles[0];
            rowTiles.Clear();
            return firstTile;
        }
    }
}
