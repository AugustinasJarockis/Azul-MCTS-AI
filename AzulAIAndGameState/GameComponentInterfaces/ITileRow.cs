using AzulBoardGame.Enums;

namespace AzulBoardGame.GameComponentInterfaces
{
    public interface ITileRow
    {
        public TileType? RowTileType { get; }
        public bool IsFull { get; }
        public bool IsEmpty { get; }
        public int TileCount { get; }
        public int FreeSlotCount { get; }
    }
}
