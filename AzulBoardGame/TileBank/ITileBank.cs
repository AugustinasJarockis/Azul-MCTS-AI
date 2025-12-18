using AzulBoardGame.Enums;
using System.Collections.ObjectModel;

namespace AzulBoardGame
{
    internal interface ITileBank
    {
        public ReadOnlyCollection<int> TileReserve { get; }
        public ReadOnlyCollection<int> TileDiscard { get; }

        public int ReserveSize { get; }
        public int DiscardSize { get; }

        public List<TileType> RefreshTiles(int plateCount);

        public void DiscardTiles(TileType type, int count = 1);
    }
}
