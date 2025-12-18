using AzulBoardGame.Enums;
using System.Collections.ObjectModel;

namespace AzulBoardGame
{
    internal class TileBank : ITileBank
    {
        private List<int> tileReserve = [20, 20, 20, 20, 20];
        private List<int> tileDiscard = [0, 0, 0, 0, 0];

        public ReadOnlyCollection<int> TileReserve { get; }
        public ReadOnlyCollection<int> TileDiscard { get; }

        public int ReserveSize => tileReserve.Sum();
        public int DiscardSize => tileDiscard.Sum();

        public TileBank() {
            TileReserve = new(tileReserve);
            TileDiscard = new(tileDiscard);
        }

        public DeterministicTileBank GetDeterministicCopy() { // TODO: Truksta determinizmo naudojimo
            return new([.. tileReserve], [.. tileDiscard]);
        }

        public List<TileType> RefreshTiles(int plateCount) {
            List<TileType> newTiles = [];

            Random random = new(DateTime.Now.Microsecond * DateTime.Now.Millisecond);
            
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
            for(int i = 0; i < tileReserve.Count; i++) {
                tileReserve[i] += tileDiscard[i];
                tileDiscard[i] = 0;
            }
        }
    } 
}
