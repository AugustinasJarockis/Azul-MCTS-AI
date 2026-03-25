using AzulBoardGame.Enums;
using System.Collections.ObjectModel;

namespace AzulBoardGame.PlayerBoard.PlayerTileGrid
{
    internal class TileGrid : ITileGrid
    {
        public TileGridState State { get; set; }

        private List<List<Tile?>> doneTiles = [
            [null, null, null, null, null],
            [null, null, null, null, null],
            [null, null, null, null, null],
            [null, null, null, null, null],
            [null, null, null, null, null]
           ];

        public TileGrid(TileGridState state) {
            State = state;
        }

        public List<List<TileType?>> DoneTiles => State.DoneTiles;

        public bool RowHasType(int rowNr, TileType type) => State.RowHasType(rowNr, type);
        public bool RowIsFull(int rowNr) => State.RowIsFull(rowNr);
        public bool CollumnIsFull(int collumnNr) => State.CollumnIsFull(collumnNr);
        public bool TypeIsComplete(TileType type) => State.TypeIsComplete(type);

        public int AddTile(int rowNr, Tile tile) {
            for (int i = 0; i < doneTiles[rowNr].Count; i++) {
                if (State.acceptedTiles[rowNr][i] == tile.TileType) {
                    doneTiles[rowNr][i] = tile;
                    tile.Move(0.525 + i * 0.093, 0.055 + rowNr * 0.1425);
                    return State.AddTile(rowNr, tile.TileType);
                }
            }
            throw new Exception("Failed to add tile to tile grid. Impossible state reached.");
        }
    }
}