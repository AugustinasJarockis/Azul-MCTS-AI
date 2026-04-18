using AzulBoardGame.Enums;

namespace AzulBoardGame.GameComponentInterfaces
{
    public interface ITileGrid
    {
        public List<List<TileType?>> DoneTiles { get; }
        public bool RowHasType(int rowNr, TileType type);
        public bool RowIsFull(int rowNr);
        public bool CollumnIsFull(int collumnNr);
        public bool TypeIsComplete(TileType type);
    }
}
