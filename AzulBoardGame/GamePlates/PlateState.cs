using AzulBoardGame.Enums;

namespace AzulBoardGame.GamePlates
{
    internal class PlateState : IPlate
    {
        private List<TileType> tiles = [];

        private Action<List<TileType>> TransferTiles;
        private Action<List<TileType>>? TileSelectionCallback = null;

        public List<TileType> TileTypes => tiles;
        public int TileCount => tiles.Count;
        public bool IsEmpty => tiles.Count == 0;

        public PlateState(Action<List<TileType>> transferTilesFunc) {
            TransferTiles = transferTilesFunc;
        }

        public PlateState(Action<List<TileType>> transferTilesFunc, List<TileType> tiles) {
            TransferTiles = transferTilesFunc;
            this.tiles = [..tiles];
        }

        public PlateState Copy(Action<List<TileType>> transferTilesFunc) {
            return new(transferTilesFunc, tiles);
        }

        public void PlaceTiles(List<TileType> tileTypes) => tiles.AddRange(tileTypes);

        public void SelectTiles(TileType type) {
            if (TileSelectionCallback != null) {
                List<TileType> selectedTiles = [];

                for (int i = 0; i < tiles.Count; i++) {
                    if (tiles[i] == type) {
                        selectedTiles.Add(tiles[i]);
                        tiles.Remove(tiles[i]);
                        i--;
                    }
                }

                TransferTiles(tiles);
                tiles.Clear();
                TileSelectionCallback(selectedTiles);
            }
        }

        public void SetSelectionCallback(Action<List<TileType>> tileSelectionCallback) => TileSelectionCallback = tileSelectionCallback;
        public void ClearSelectionCallback() => TileSelectionCallback = null;
    }
}
