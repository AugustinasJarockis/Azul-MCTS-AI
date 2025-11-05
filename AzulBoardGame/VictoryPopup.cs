using AzulBoardGame.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AzulBoardGame
{
    internal class VictoryPopup
    {
        private readonly Canvas _mainCanvas;
        private readonly Action RestartGame;

        private Border border;
        private Canvas popupCanvas;
        private TextBlock victoryText;
        private Image restartButton;
        public VictoryPopup(Canvas mainCanvas, Action restartGame) {
            _mainCanvas = mainCanvas;
            RestartGame = restartGame;
            
            popupCanvas = new();

            var darkBrownBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#722F27"));

            border = new Border {
                BorderBrush = darkBrownBrush,
                BorderThickness = new Thickness(8),
                CornerRadius = new CornerRadius(5),
                Child = popupCanvas,
                Visibility = Visibility.Hidden,
            };

            victoryText = new TextBlock {
                Text = "",
                Foreground = darkBrownBrush,
                FontSize = 30,
                TextAlignment = TextAlignment.Center
            };

            Image victoryBoard = new Image {
                Stretch = Stretch.Fill,
                Source = new BitmapImage(new Uri("Textures/victoryPlank.png", UriKind.Relative)),
            };


            Panel.SetZIndex(border, 10);

            _mainCanvas.Loaded += (s, e) => {
                _mainCanvas.SetRelativePosCentered(border, 0.5, 0.5, 0.25, 0.4);
            };

            restartButton = new Image {
                Source = new BitmapImage(new Uri("Textures/restart.png", UriKind.Relative)),
            };

            popupCanvas.Loaded += (s, e) => {
                popupCanvas.Dispatcher.BeginInvoke(() => {
                    popupCanvas.SetRelativePosCentered(restartButton, 0.5, 0.8, 0.3, 0.7);
                    popupCanvas.SetRelativePosCentered(victoryText, 0.5, 0.3, 0.5, 0.8);
                    popupCanvas.SetRelativeDimensions(victoryBoard, 1, 1);
                });
            };

            restartButton.MouseDown += (s, a) => { Hide();  RestartGame(); };
            restartButton.MouseEnter += (s, a) => restartButton.Opacity = 0.5;
            restartButton.MouseLeave += (s, a) => restartButton.Opacity = 1.0;

            popupCanvas.Children.Add(victoryBoard);
            popupCanvas.Children.Add(restartButton);
            popupCanvas.Children.Add(victoryText);
            _mainCanvas.Children.Add(border);
        }

        public void Show() => border.Visibility = Visibility.Visible;

        public void Show(string victorName, int pointCount) {
            victoryText.Text = victorName + " won!\n Points: " + pointCount;
            border.Visibility = Visibility.Visible;
        }

        public void Hide() => border.Visibility = Visibility.Collapsed;
    }
}
