using AzulBoardGame.Enums;

namespace AzulBoardGame
{
    internal interface ITileContainer
    {
        public void HighlightTiles(TileType type);
        public void UnhighlightTiles(TileType type);
        public void SelectTiles(TileType type);
    }
}
