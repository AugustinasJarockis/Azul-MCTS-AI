using AzulBoardGame.Enums;
using AzulBoardGame.Extensions;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AzulBoardGame
{
    internal class Player : ITileContainer
    {
        private readonly Canvas _mainCanvas;
        private readonly ScaleTransform _scaleTransform;
        private readonly TranslateTransform _translateTransform;
        private readonly Key _keyToFocus;
        private Canvas _playerCanvas;

        public Player(
            Canvas mainCanvas, 
            ScaleTransform scaleTransform, 
            TranslateTransform translateTransform, 
            string name, 
            PlayerType playerType,
            Key keyToFocus,
            double xPos, 
            double yPos, 
            double size
            ) {
            _mainCanvas = mainCanvas;
            _scaleTransform = scaleTransform;
            _translateTransform = translateTransform;
            _keyToFocus = keyToFocus;

            _playerCanvas = new() {
                Name = name + "Canvas"
            };

            _mainCanvas.Loaded += (s, e) => {
                _mainCanvas.SetRelativePosCentered(_playerCanvas, xPos, yPos, size, size);
            };

            Image playerBoard = new Image {
                Name = "Player" + name,
                Opacity = 0.65,
                Stretch = Stretch.Fill,
                Source = new BitmapImage(new Uri("Textures/playerBoard.jpg", UriKind.Relative)),
            };

            _playerCanvas.Loaded += (s, e) => {
                _playerCanvas.Dispatcher.BeginInvoke(() => {
                    _playerCanvas.SetRelativeDimensions(playerBoard, 1, 1);
                });
            };

            _playerCanvas.Children.Add(playerBoard);
            _mainCanvas.Children.Add(_playerCanvas);

            _mainCanvas.KeyDown += (s, e) => {
                if (e.Key == _keyToFocus)
                    Focus();
            };
        }

        public void SelectTiles(TileType type) {}
        public void HighlightTiles(TileType type) {}
        public void UnhighlightTiles(TileType type) {}

        public void Focus() {
            _translateTransform.X = -Canvas.GetLeft(_playerCanvas);
            _translateTransform.Y = -Canvas.GetTop(_playerCanvas);
            
            _scaleTransform.ScaleX = _mainCanvas.ActualWidth / _playerCanvas.ActualWidth;
            _scaleTransform.ScaleY = _scaleTransform.ScaleX;
        }
    }
}
