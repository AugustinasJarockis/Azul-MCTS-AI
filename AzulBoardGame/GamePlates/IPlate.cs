using AzulBoardGame.Enums;

namespace AzulBoardGame.GamePlates
{
    internal interface IPlate
    {
        public List<TileType> TileTypes { get; }
        public int TileCount { get; }
        public bool IsEmpty { get; }

        public void PlaceTiles(List<TileType> tileTypes);
        public void SelectTiles(TileType type);
    }
}
