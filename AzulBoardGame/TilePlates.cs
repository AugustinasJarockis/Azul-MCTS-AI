using AzulBoardGame.Enums;
using AzulBoardGame.Extensions;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace AzulBoardGame
{
    internal class TilePlates : ITileContainer
    {
        private readonly Canvas _mainCanvas;
        private readonly ScaleTransform _scaleTransform;
        private readonly TranslateTransform _translateTransform;
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

        private Canvas _centerCanvas;

        private Tile? firstTile = null;
        private List<Tile> centerTiles = [];

        public List<Plate> Plates { get; set; } = [];
        public int CenterTileCount => centerTiles.Count + (firstTile != null ? 1 : 0);
        public int TotalTileCount => CenterTileCount + Plates.Sum(p => p.TileCount);

        public TilePlates(
            Canvas mainCanvas, 
            ScaleTransform scaleTransform, 
            TranslateTransform translateTransform, 
            Key keyToFocus, 
            int tileCount
            ) {
            _mainCanvas = mainCanvas;
            _scaleTransform = scaleTransform;
            _translateTransform = translateTransform;
            _keyToFocus = keyToFocus;

            _centerCanvas = new();

            _mainCanvas.Loaded += (s, e) => {
                _mainCanvas.SetRelativePosCenteredSquare(_centerCanvas, 0.5, 0.5, 0.35);
            };

            _centerCanvas.Loaded += (s, e) => {
                _centerCanvas.Dispatcher.BeginInvoke(() => {
                    for (int i = 0; i < tileCount; i++)
                        Plates.Add(new(
                            _centerCanvas,
                            TransferTilesToCenter,
                            (Math.Cos((2 * Math.PI / tileCount) * i) + 1) / 2,
                            (Math.Sin((2 * Math.PI / tileCount) * i) + 1) / 2
                            ));
                },
                    DispatcherPriority.Loaded
                );
            };

            _mainCanvas.Children.Add(_centerCanvas);

            _mainCanvas.KeyDown += (s, e) => {
                if (e.Key == _keyToFocus)
                    Focus();
            };
        }

        public void TransferTilesToCenter(List<Tile> tiles) {            
            foreach (Tile tile in tiles) {
                if (CenterTileCount > 29)
                    throw new Exception("The number of center tiles should never exceed 28");
                
                (int xPos, int yPos) = centerTilePositions[CenterTileCount];

                tile.Move(_centerCanvas, this, 0.1 * xPos + 0.5, -0.1 * yPos + 0.5, tileSize);

                centerTiles.Add(tile);
            }
        }

        public void RearrangeCenterTiles() {
            if (firstTile != null)
                firstTile.Move(0.5, 0.5);

            int firstTileCount = firstTile != null ? 1 : 0;

            for (int i = 0; i < centerTiles.Count; i++) {
                (int xPos, int yPos) = centerTilePositions[i + firstTileCount];
                centerTiles[i].Move(0.1 * xPos + 0.5, -0.1 * yPos + 0.5);
            }
        }

        public void RefreshPlates(List<TileType> tileTypes) {
            for (int i = 0; i < (tileTypes.Count + 3) / 4; i++)
                Plates[i].PlaceTiles([.. tileTypes.Skip(i * 4).Take(4)]);

            firstTile = new Tile(_centerCanvas, this, TileType.First, 0.5, 0.5, tileSize);
        }

        public void SelectTiles(TileType type) {

        }

        public void HighlightTiles(TileType type) {
            if (firstTile != null)
                firstTile.ShowBorder();
            
            foreach (Tile tile in centerTiles) {
                if (tile.TileType == type)
                    tile.ShowBorder();
            }
        }

        public void UnhighlightTiles(TileType type) {
            if (firstTile != null)
                firstTile.HideBorder();

            foreach (Tile tile in centerTiles) {
                if (tile.TileType == type)
                    tile.HideBorder();
            }
        }

        public void EnableUserInput() {
            foreach (Plate plate in Plates)
                plate.EnableUserInput();

            foreach (Tile tile in centerTiles)
                tile.StartMouseInput();
        }
        public void DisableUserInput() {
            foreach(Plate plate in Plates)
                plate.DisableUserInput();

            foreach (Tile tile in centerTiles)
                tile.StopMouseInput();
        }

        public void Focus() {
            double canvasViewRatio = 0.7;

            _translateTransform.X = 
                -Canvas.GetLeft(_centerCanvas) 
                + (_centerCanvas.ActualWidth * ((1 / canvasViewRatio) * _mainCanvas.ActualWidth / _mainCanvas.ActualHeight - 1)) / 2;
            
            _translateTransform.Y = 
                -Canvas.GetTop(_centerCanvas)
                + (_centerCanvas.ActualWidth * (1 / canvasViewRatio - 1)) / 2;

            _scaleTransform.ScaleY = canvasViewRatio * _mainCanvas.ActualHeight / _centerCanvas.ActualHeight;
            _scaleTransform.ScaleX = _scaleTransform.ScaleY;
        }
    }
}
