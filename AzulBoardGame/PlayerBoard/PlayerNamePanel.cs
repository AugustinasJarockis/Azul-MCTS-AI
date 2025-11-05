using AzulBoardGame.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AzulBoardGame.PlayerBoard
{
    internal class PlayerNamePanel
    {
        private readonly Canvas _playerCanvas;

        private TextBlock nameText;
        private TextBlock turnMarkerText;

        public string Name => nameText.Text;
        public PlayerNamePanel(Canvas playerCanvas, string name, Brush colourBrush) {
            _playerCanvas = playerCanvas;

            nameText = new TextBlock {
                Text = name.Substring(0, Math.Min(14, name.Length)),
                Foreground = colourBrush
            };
            turnMarkerText = new TextBlock {
                Text = "*",
                Foreground = Brushes.Gold,
                Visibility = Visibility.Hidden,
                FontSize = 30
            };

            Panel.SetZIndex(nameText, 10);
            Panel.SetZIndex(turnMarkerText, 10);

            _playerCanvas.Loaded += (s, e) => {
                _playerCanvas.Dispatcher.BeginInvoke(() => {
                    _playerCanvas.SetRelativePosCentered(nameText, 0.3, 0.225, 0.3, 0.5);
                    _playerCanvas.SetRelativePosCentered(turnMarkerText, 0.375, 0.19, 0.3, 0.2);
                });
            };

            _playerCanvas.Children.Add(nameText);
            _playerCanvas.Children.Add(turnMarkerText);
        }

        public void ShowPlayerTurn() => turnMarkerText.Visibility = Visibility.Visible;
        public void HidePlayerTurn() => turnMarkerText.Visibility = Visibility.Hidden;
    }
}
