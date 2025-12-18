using AzulBoardGame.Enums;

namespace AzulBoardGame.PlayerBoard.PlayerTileGrid
{
    internal interface ITileGrid
    {
        public List<List<TileType?>> DoneTiles { get; }
        public bool RowHasType(int rowNr, TileType type);
        public bool RowIsFull(int rowNr);
        public bool CollumnIsFull(int collumnNr);
        public bool TypeIsComplete(TileType type);
    }
}
