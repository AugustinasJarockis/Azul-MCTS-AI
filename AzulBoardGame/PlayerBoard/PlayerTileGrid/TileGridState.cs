using AzulBoardGame.Enums;
using System.Collections.ObjectModel;

namespace AzulBoardGame.PlayerBoard.PlayerTileGrid
{
    internal class TileGridState : ITileGrid
    {
        private ReadOnlyCollection<ReadOnlyCollection<TileType>> acceptedTiles = new([
            new([TileType.Cyan, TileType.Brown, TileType.White, TileType.Black, TileType.Red]),
            new([TileType.Red, TileType.Cyan, TileType.Brown, TileType.White, TileType.Black]),
            new([TileType.Black, TileType.Red, TileType.Cyan, TileType.Brown, TileType.White]),
            new([TileType.White, TileType.Black, TileType.Red, TileType.Cyan, TileType.Brown]),
            new([TileType.Brown, TileType.White, TileType.Black, TileType.Red, TileType.Cyan])
           ]);

        private List<List<TileType?>> doneTiles = [
            [null, null, null, null, null],
            [null, null, null, null, null],
            [null, null, null, null, null],
            [null, null, null, null, null],
            [null, null, null, null, null]
           ];

        public List<List<TileType?>> DoneTiles => doneTiles;

        public TileGridState () {}
        public TileGridState (List<List<TileType?>> doneTiles) {
            this.doneTiles = [];
            foreach (var line in doneTiles) {
                this.doneTiles.Add([.. line]);
            }
        }

        public TileGridState Copy() {
            return new TileGridState(doneTiles);
        }
        public bool RowHasType(int rowNr, TileType type) => doneTiles[rowNr].Contains(type);
        public bool RowIsFull(int rowNr) => !doneTiles[rowNr].Contains(null);
        public bool CollumnIsFull(int collumnNr) => !doneTiles.Select(r => r[collumnNr]).Contains(null);
        public bool TypeIsComplete(TileType type) => doneTiles.Count(r => r.Contains(type)) == 5;

        public int AddTile(int rowNr, TileType tile) {
            for (int i = 0; i < doneTiles[rowNr].Count; i++) {
                if (acceptedTiles[rowNr][i] == tile) {
                    doneTiles[rowNr][i] = tile;
                    return CountTilePoints(rowNr, i);
                }
            }
            throw new Exception("Failed to add tile to tile grid. Impossible state reached.");
        }

        private int CountTilePoints(int rowNr, int collumnNr) {
            int pointSum = 0;

            int collumnPoints = 0;
            for (int i = rowNr + 1; i < doneTiles.Count && doneTiles[i][collumnNr] != null; i++)
                collumnPoints++;

            for (int i = rowNr - 1; i >= 0 && doneTiles[i][collumnNr] != null; i--)
                collumnPoints++;

            pointSum += collumnPoints + (collumnPoints == 0 ? 0 : 1);

            int rowPoints = 0;
            for (int i = collumnNr + 1; i < doneTiles[rowNr].Count && doneTiles[rowNr][i] != null; i++)
                rowPoints++;

            for (int i = collumnNr - 1; i >= 0 && doneTiles[rowNr][i] != null; i--)
                rowPoints++;

            pointSum += rowPoints + (rowPoints == 0 ? 0 : 1);

            return Math.Max(pointSum, 1);
        }
    }
}
