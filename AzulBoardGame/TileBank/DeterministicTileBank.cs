using AzulBoardGame.Enums;
using System.Collections.ObjectModel;

namespace AzulBoardGame
{
    internal class DeterministicTileBank : ITileBank
    {
        private readonly int[] tileReserve = [20, 20, 20, 20, 20];
        private readonly int[] tileDiscard = [0, 0, 0, 0, 0];

        public int TileSelectionOutcomeSeed { get; set; }

        public ReadOnlyCollection<int> TileReserve { get; }
        public ReadOnlyCollection<int> TileDiscard { get; }

        public int ReserveSize => tileReserve.Sum();
        public int DiscardSize => tileDiscard.Sum();

        public DeterministicTileBank() {
            TileReserve = new(tileReserve);
            TileDiscard = new(tileDiscard);
        }

        public DeterministicTileBank(int[] tileReserve, int[] tileDiscard) {
            this.tileReserve = new int[5];
            this.tileDiscard = new int[5];
            for (int i = 0; i < 5; i++) {
                this.tileReserve[i] = tileReserve[i];
                this.tileDiscard[i] = tileDiscard[i];
            }
            TileReserve = new(this.tileReserve);
            TileDiscard = new(this.tileDiscard);
        }

        public DeterministicTileBank Copy() {
            return new (tileReserve, tileDiscard);
        }

        public List<TileType> RefreshTiles(int plateCount) {
            List<TileType> newTiles = [];

            Random random = new(TileSelectionOutcomeSeed);

            for (int i = 0; i < plateCount * 4; i++) {
                if (ReserveSize == 0) {
                    ReuseDiscardedTiles();
                    if (ReserveSize == 0)
                        break;
                }


                int tileToPick = random.Next(ReserveSize);
                int tileSum = tileReserve[0];

                for (int type = 1; type < 6; type++) {
                    if (tileToPick < tileSum) {
                        newTiles.Add((TileType)type);
                        tileReserve[type - 1]--;
                        break;
                    }
                    tileSum += tileReserve[type];
                }
            }
            return newTiles;
        }

        public void DiscardTiles(TileType type, int count = 1) => tileDiscard[(int)type - 1] += count;

        private void ReuseDiscardedTiles() {
            for (int i = 0; i < 5; i++) {
                tileReserve[i] += tileDiscard[i];
                tileDiscard[i] = 0;
            }
        }
    }
}
