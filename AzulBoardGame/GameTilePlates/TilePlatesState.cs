using AzulBoardGame.Enums;
using AzulBoardGame.GamePlates;
using AzulBoardGame.Players.MCTS;

namespace AzulBoardGame.GameTilePlates
{
    internal class TilePlatesState
    {
        private readonly GameState _gameManager;

        private bool firstTileExist = false;
        private List<TileType> centerTiles = [];

        private Action<List<TileType>>? TileSelectionCallback = null;

        private List<PlateState> plates = [];
        public List<IPlate> Plates => [.. plates];
        public int StartingPlayer { get; private set; } = 0;
        public List<TileType> CenterTileTypes => centerTiles;
        public int CenterTileCount => centerTiles.Count;
        public int TotalTileCount => CenterTileCount + Plates.Sum(p => p.TileCount);
        public bool FirstTileExists => firstTileExist;

        public TilePlatesState(
            GameState gameManager,
            int plateCount
            ) {
            _gameManager = gameManager;

            for (int i = 0; i < plateCount; i++)
                plates.Add(new PlateState(TransferTilesToCenter));
        }

        public TilePlatesState(
            GameState gameManager,
            bool firstTileExist,
            List<TileType> centerTiles,
            List<PlateState> plates,
            int startingPlayer
            ) {
            _gameManager = gameManager;
            this.firstTileExist = firstTileExist;
            this.centerTiles = [..centerTiles];
            StartingPlayer = startingPlayer;

            foreach (var plate in plates)
                this.plates.Add(plate.Copy(TransferTilesToCenter));
        }

        public TilePlatesState(
            GameState gameManager,
            bool firstTileExist,
            List<TileType> centerTiles,
            List<Plate> plates,
            int startingPlayer
            ) {
            _gameManager = gameManager;
            this.firstTileExist = firstTileExist;
            this.centerTiles = [.. centerTiles];
            StartingPlayer = startingPlayer;

            foreach (var plate in plates)
                this.plates.Add(plate.GetState(TransferTilesToCenter));
        }

        public TilePlatesState Copy(GameState gameManager) {
            return new(gameManager, firstTileExist, centerTiles, plates, StartingPlayer);
        }
        private void TransferTilesToCenter(List<TileType> tiles) => centerTiles.AddRange(tiles);

        public void RefreshPlates(List<TileType> tileTypes) {
            for (int i = 0; i < (tileTypes.Count + 3) / 4; i++)
                plates[i].PlaceTiles([.. tileTypes.Skip(i * 4).Take(4)]);

            firstTileExist = true;
        }

        public void SelectTiles(TileType type) {

            List<TileType> selectedTiles = [.. centerTiles.Where(t => t == type)];
            foreach (TileType tile in selectedTiles) {
                centerTiles.Remove(tile);
            }

            if (firstTileExist) {
                StartingPlayer = _gameManager.CurrentPlayer;
                selectedTiles.Add(TileType.First);
                firstTileExist = false;
            }

            TileSelectionCallback?.Invoke(selectedTiles);
        }

        public void SetSelectionCallback(Action<List<TileType>> tileSelectionCallback) {
            TileSelectionCallback = tileSelectionCallback;
            foreach (PlateState plate in plates) {
                plate.SetSelectionCallback(tileSelectionCallback);
            }
        }
        public void ClearSelectionCallback() {
            TileSelectionCallback = null;
            foreach (PlateState plate in plates) {
                plate.ClearSelectionCallback();
            }
        }
    }
}
