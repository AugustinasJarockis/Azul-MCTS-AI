using AzulBoardGame.Enums;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AzulBoardGame.PlayerBoard.PlayerTileRow
{
    internal interface ITileRow
    {
        public TileType? rowTileType { get; }
        public bool IsFull { get; }
        public bool IsEmpty { get; }
        public int TileCount { get; }
        public int FreeSlotCount { get; }
    }
}
