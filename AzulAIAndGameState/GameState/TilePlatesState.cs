using AzulBoardGame.Enums;

namespace AzulBoardGame.GameState
{
    public class TilePlatesState
    {
        public bool firstTileExist = true;
        private List<TileType> centerTiles = [];

        public List<PlateState> Plates = [];
        public List<TileType> CenterTileTypes => centerTiles;
        public int CenterTileCount => centerTiles.Count;
        public int TotalTileCount => CenterTileCount + Plates.Sum(p => p.TileCount);

        public TilePlatesState (int plateCount) {
            for (int i = 0; i < plateCount; i++) {
                Plates.Add(new());
            }
        }

        public TilePlatesState(List<PlateState> platesCopy, List<TileType> centerTilesCopy, bool firstTileExistValue) {
            firstTileExist = firstTileExistValue;
            Plates = platesCopy;
            centerTiles = centerTilesCopy;
        }

        public TilePlatesState Copy() {
            List<PlateState> platesCopy = [..Plates.Select(p => p.Copy())];
            return new(platesCopy, [..centerTiles], firstTileExist);
        }

        public void Reset() {
            foreach (var plate in Plates)
                plate.Clear();

            centerTiles.Clear();
            firstTileExist = true;
        }

        //NOTE: Šita funkcija veikia tik su 2 žaidėjais kol kas, nes permažai centre plytelių gražina
        public (List<int>, List<List<int>>) GetListState() {
            List<int> centerStateList = [];

            if (firstTileExist)
                centerStateList.Add(1);
            else 
                centerStateList.Add(0);

            for (int i = 1; i < 16; i++) {
                if (centerTiles.Count > i)
                    centerStateList.Add((int)centerTiles[i]);
                else
                    centerStateList.Add(0);
            }

            return (centerStateList, [.. Plates.Select(p => p.GetListState())]);
        }

        public void TransferTilesToCenter(List<TileType> tiles) => centerTiles.AddRange(tiles);

        public void RefreshPlates(List<TileType> tileTypes) {
            for (int i = 0; i < (tileTypes.Count + 3) / 4; i++)
                Plates[i].PlaceTiles([.. tileTypes.Skip(i * 4).Take(4)]);

            centerTiles.Clear();
            firstTileExist = true;
        }

        public List<TileType> SelectTiles(TileType type, int plateNr) {

            List<TileType> selectedTiles;

            if (plateNr == 0) {
                selectedTiles = [.. centerTiles.Where(t => t == type)];
                centerTiles = [..centerTiles.Where(t => t != type)];

                if (firstTileExist) {
                    selectedTiles.Add(TileType.First);
                    firstTileExist = false;
                }
            }
            else {
                (selectedTiles, var unselectedTiles) = Plates[plateNr - 1].SelectTiles(type);
                TransferTilesToCenter(unselectedTiles);
            }
            return selectedTiles;
        }
    }
}
