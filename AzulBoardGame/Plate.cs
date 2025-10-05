using AzulBoardGame.Enums;
using AzulBoardGame.Extensions;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AzulBoardGame
{
    internal class Plate : ITileContainer
    {
        private readonly Canvas _centerCanvas;

        private Canvas _plateCanvas;

        private List<Tile> tiles = [];

        private Action<List<Tile>> TransferTiles;
        private Action<List<Tile>>? TileSelectionCallback = null;

        public List<TileType> TyleTypes => tiles.Select(t => t.TileType).ToList();
        public int TileCount => tiles.Count;

        public Plate(Canvas centerCanvas, Action<List<Tile>> transferTilesFunc, double xPos, double yPos) { 
            _centerCanvas = centerCanvas;
            _plateCanvas = new();

            TransferTiles = transferTilesFunc;

            _centerCanvas.SetRelativePosCenteredSquare(_plateCanvas, xPos, yPos, 0.3);

            var plateImage = new Image {
                Source = new BitmapImage(new Uri("Textures/mandala.png", UriKind.Relative)),
                Opacity = 0.8,
                Stretch = Stretch.Fill
            };

            _plateCanvas.Loaded += (s, e) => {
                _plateCanvas.Dispatcher.BeginInvoke(() => {
                    _plateCanvas.SetRelativeDimensions(plateImage, 1, 1);
                });
            };

            _plateCanvas.Children.Add(plateImage);
            _centerCanvas.Children.Add(_plateCanvas);
        }

        public void PlaceTiles(List<TileType> tileTypes) {

            double tileSize = 0.32;
            List<double> xPos = [0.32, 0.68, 0.32, 0.68];
            List<double> yPos = [0.32, 0.32, 0.68, 0.68];

            for (int i = 0; i < tileTypes.Count && i < 4; i++)
                tiles.Add(new(_plateCanvas, this, tileTypes[i], xPos[i], yPos[i], tileSize));
        }

        public void SelectTiles(TileType type) {
            if (TileSelectionCallback != null) {
                List<Tile> selectedTiles = [];
            
                for (int i = 0; i < tiles.Count; i++) {
                    if (tiles[i].TileType == type) {
                        tiles[i].HideBorder();
                        tiles[i].StopMouseInput();
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

        public void HighlightTiles(TileType type) {
            foreach (Tile tile in tiles) {
                if (tile.TileType == type)
                    tile.ShowBorder();
            }
        }

        public void UnhighlightTiles(TileType type) {
            foreach (Tile tile in tiles) {
                if (tile.TileType == type)
                    tile.HideBorder();
            }
        }

        public void EnableUserInput() {
            foreach (Tile tile in tiles)
                tile.StartMouseInput();
        }
        public void DisableUserInput() {
            foreach (Tile tile in tiles)
                tile.StopMouseInput();
        }

        public void SetSelectionCallback(Action<List<Tile>> tileSelectionCallback) => TileSelectionCallback = tileSelectionCallback;
        public void ClearSelectionCallback() => TileSelectionCallback = null;
    }
}
