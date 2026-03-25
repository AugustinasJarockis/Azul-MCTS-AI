using AzulBoardGame.Enums;
using AzulBoardGame.Extensions;
using AzulBoardGame.GamePlates;
using AzulBoardGame.GameState;
using AzulBoardGame.Utilities;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace AzulBoardGame.GameTilePlates
{
    internal class TilePlates : ITileContainer
    {
        private readonly CanvasControls _canvasControls;

        private readonly GameManager _gameManager;
        private readonly Key _keyToFocus;

        private const double tileSize = 0.096;
        private List<(int, int)> centerTilePositions = [
                ( 0,  0), ( 0,  1), ( 1,  1), ( 1,  0), ( 1, -1),
                ( 0, -1), (-1, -1), (-1,  0), (-1,  1), (-1,  2),
                ( 0,  2), ( 1,  2), ( 2,  2), ( 2,  1), ( 2,  0),
                ( 2, -1), ( 2, -2), ( 1, -2), ( 0, -2), (-1, -2),
                (-2, -2), (-2, -1), (-2,  0), (-2,  1), (-2,  2),
                (-2,  3), (-1,  3), ( 0,  3), ( 1,  3), ( 2,  3),
                ];

        private Canvas centerCanvas;

        private TilePlatesState _tilePlatesState;

        private Tile? firstTile = null;
        private List<Tile> centerTiles = [];

        private Action<List<Tile>>? TileSelectionCallback = null;

        private List<Plate> plates = [];
        public List<IPlate> Plates => [.. plates];
        public List<TileType> CenterTileTypes => [..centerTiles.Select(t => t.TileType)];
        public int CenterTileCount => centerTiles.Count;
        public int TotalTileCount => CenterTileCount + Plates.Sum(p => p.TileCount);
        public bool FirstTileExists => firstTile != null;

        public TilePlates(
            CanvasControls canvasControls, 
            GameManager gameManager,
            Key keyToFocus, 
            TilePlatesState tilePlatesState
            ) {
            _canvasControls = canvasControls;
            _gameManager = gameManager;
            _keyToFocus = keyToFocus;
            _tilePlatesState = tilePlatesState;

            centerCanvas = new();

            _canvasControls.Canvas.Loaded += (s, e) => {
                _canvasControls.Canvas.SetRelativePosCenteredSquare(centerCanvas, 0.5, 0.5, 0.35);
            };

            centerCanvas.Loaded += (s, e) => {
                centerCanvas.Dispatcher.BeginInvoke(() => {
                    for (int i = 0; i < tilePlatesState.Plates.Count; i++)
                        plates.Add(new Plate(
                            centerCanvas,
                            TransferTilesToCenter,
                            (Math.Cos(2 * Math.PI / tilePlatesState.Plates.Count * i) + 1) / 2,
                            (Math.Sin(2 * Math.PI / tilePlatesState.Plates.Count * i) + 1) / 2,
                            tilePlatesState.Plates[i]
                            ));
                },
                    DispatcherPriority.Loaded
                );
            };

            _canvasControls.Canvas.Children.Add(centerCanvas);

            _canvasControls.Canvas.KeyDown += (s, e) => {
                if (e.Key == _keyToFocus)
                    Focus();
            };
        }

        private void TransferTilesToCenter(List<Tile> tiles) {
            int firstTileCount = FirstTileExists ? 1 : 0;
            _tilePlatesState.TransferTilesToCenter([.. tiles.Select(t => t.TileType)]);
            foreach (Tile tile in tiles) {

                if (CenterTileCount + firstTileCount > 29)
                    throw new Exception("The number of center tiles should never exceed 28");
                
                (int xPos, int yPos) = centerTilePositions[CenterTileCount + firstTileCount];

                tile.Move(centerCanvas, this, 0.1 * xPos + 0.5, -0.1 * yPos + 0.5, tileSize);

                centerTiles.Add(tile);
            }
        }

        public void RearrangeCenterTiles() {
            firstTile?.Move(0.5, 0.5);

            int firstTileCount = firstTile != null ? 1 : 0;

            for (int i = 0; i < centerTiles.Count; i++) {
                (int xPos, int yPos) = centerTilePositions[i + firstTileCount];
                centerTiles[i].MoveCentered(0.1 * xPos + 0.5, -0.1 * yPos + 0.5, tileSize);
            }
        }

        public void RefreshPlates(List<TileType> tileTypes) {
            for (int i = 0; i < (tileTypes.Count + 3) / 4; i++)
                plates[i].PlaceTiles([.. tileTypes.Skip(i * 4).Take(4)]);

            firstTile ??= new Tile(centerCanvas, this, TileType.First, 0.5, 0.5, tileSize);
            _tilePlatesState.firstTileExist = true;
        }

        public void SelectTiles(TileType type) {

            _tilePlatesState.SelectTiles(type, 0);

            List<Tile> selectedTiles = [.. centerTiles.Where(t => t.TileType == type)];
            foreach(Tile tile in selectedTiles) {
                tile.HideBorder();
                tile.StopMouseInput();
                centerTiles.Remove(tile);
            }

            if (firstTile != null) {
                selectedTiles.Add(firstTile);
                firstTile.HideBorder();
                firstTile.StopMouseInput();
                firstTile = null;
            }

            RearrangeCenterTiles();

            TileSelectionCallback?.Invoke(selectedTiles);
        }

        public void HighlightTiles(TileType type) {
            firstTile?.ShowBorder();
            
            foreach (Tile tile in centerTiles) {
                if (tile.TileType == type)
                    tile.ShowBorder();
            }
        }

        public void UnhighlightTiles(TileType type) {
            firstTile?.HideBorder();

            foreach (Tile tile in centerTiles) {
                if (tile.TileType == type)
                    tile.HideBorder();
            }
        }

        public void EnableUserInput() {
            foreach (Plate plate in plates)
                plate.EnableUserInput();

            foreach (Tile tile in centerTiles)
                tile.StartMouseInput();
        }
        public void DisableUserInput() {
            foreach(Plate plate in plates)
                plate.DisableUserInput();

            foreach (Tile tile in centerTiles)
                tile.StopMouseInput();
        }

        public void SetSelectionCallback(Action<List<Tile>> tileSelectionCallback) {
            TileSelectionCallback = tileSelectionCallback;
            foreach (Plate plate in plates) {
                plate.SetSelectionCallback(tileSelectionCallback);
            }
        }
        public void ClearSelectionCallback() {
            TileSelectionCallback = null;
            foreach (Plate plate in plates) {
                plate.ClearSelectionCallback();
            }
        }

        public void Focus() {
            double canvasViewRatio = 0.7;

            _canvasControls.TranslateTransform.X = 
                -Canvas.GetLeft(centerCanvas) 
                + centerCanvas.ActualWidth * (1 / canvasViewRatio * _canvasControls.Canvas.ActualWidth / _canvasControls.Canvas.ActualHeight - 1) / 2;

            _canvasControls.TranslateTransform.Y = 
                -Canvas.GetTop(centerCanvas)
                + centerCanvas.ActualWidth * (1 / canvasViewRatio - 1) / 2;

            _canvasControls.ScaleTransform.ScaleY = canvasViewRatio * _canvasControls.Canvas.ActualHeight / centerCanvas.ActualHeight;
            _canvasControls.ScaleTransform.ScaleX = _canvasControls.ScaleTransform.ScaleY;
        }
    }
}
