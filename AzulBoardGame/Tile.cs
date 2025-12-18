using AzulBoardGame.Enums;
using AzulBoardGame.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AzulBoardGame
{
    internal class Tile
    {
        private Canvas _canvas;
        private ITileContainer _container;

        private static readonly List<BitmapImage> bitmapSources = [
            new BitmapImage(new Uri("Textures/firstTile.png", UriKind.Relative)),
            new BitmapImage(new Uri("Textures/whiteTile.png", UriKind.Relative)),
            new BitmapImage(new Uri("Textures/brownTile.png", UriKind.Relative)),
            new BitmapImage(new Uri("Textures/redTile.png",   UriKind.Relative)),
            new BitmapImage(new Uri("Textures/blackTile.png", UriKind.Relative)),
            new BitmapImage(new Uri("Textures/cyanTile.png",  UriKind.Relative))
            ];

        private Image tileImage;
        private Border tileBorder;
        public TileType TileType { get; }

        public Tile(Canvas canvas, ITileContainer container, TileType type, double xPos, double yPos, double size) {
            _canvas = canvas;
            _container = container;
            TileType = type;

            tileImage = new Image {
                Source = bitmapSources[(int)type],
                Stretch = Stretch.Fill,
                IsHitTestVisible = false
            };

            tileBorder = new Border {
                BorderBrush = Brushes.GreenYellow,
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(5),
                Child = tileImage
            };

            _canvas.SetRelativePosCenteredSquare(tileBorder, xPos, yPos, size);
            _canvas.Children.Add(tileBorder);

            tileImage.MouseEnter += (o, e) => {
                _container.HighlightTiles(TileType);
            };
            tileImage.MouseLeave += (o, e) => {
                _container.UnhighlightTiles(TileType);
            };
            tileImage.MouseDown += (o, e) => {
                if (e.LeftButton == MouseButtonState.Pressed)
                    _container.SelectTiles(TileType);
            };
        }

        public void Destroy() {
            tileBorder.Child = null;
            _canvas.Children.Remove(tileBorder);
        }
        
        public void Move(Canvas newCanvas, ITileContainer newContainer, double xPos, double yPos, double size) {
            _canvas.Children.Remove(tileBorder);

            _canvas = newCanvas;
            _container = newContainer;

            _canvas.SetRelativePosCenteredSquare(tileBorder, xPos, yPos, size);
            _canvas.Children.Add(tileBorder);
        }

        public void Move(double xPos, double yPos) {
            _canvas.SetRelativePos(tileBorder, xPos, yPos);
        }
        public void MoveCentered(double xPos, double yPos, double tileSize) {
            _canvas.SetRelativePosCenteredSquare(tileBorder, xPos, yPos, tileSize);
        }

        public void ShowBorder() => tileBorder.BorderThickness = new Thickness(2.5);
        public void HideBorder() => tileBorder.BorderThickness = new Thickness(0);

        public void StopMouseInput() => tileImage.IsHitTestVisible = false;
        public void StartMouseInput() => tileImage.IsHitTestVisible = true;
    }
}
