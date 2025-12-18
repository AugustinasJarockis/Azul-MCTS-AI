using AzulBoardGame.Extensions;
using System.Windows.Controls;
using System.Windows.Media;

namespace AzulBoardGame.PlayerBoard.PointCounter
{
    internal class PointCounter : IPointCounter
    {
        private readonly Canvas _playerCanvas;

        private TextBlock pointText;
        public int Points { get; private set; } = 0;
        public PointCounter(Canvas playerCanvas) { 
            _playerCanvas = playerCanvas;

            pointText = new TextBlock {
                Text = "Points: " + (Points >= 100 ? "" : " ") + (Points >= 10 ? "" : " ") + Points.ToString(),
                Foreground = Brushes.White
            };

            Panel.SetZIndex(pointText, 10);

            _playerCanvas.Loaded += (s, e) => {
                _playerCanvas.Dispatcher.BeginInvoke(() => {
                    _playerCanvas.SetRelativePosCentered(pointText, 0.1825, 0.31, 0.2, 0.3);
                });
            };
            
            _playerCanvas.Children.Add(pointText);
        }

        public void UpdatePoints(int pointsChange) {
            Points += pointsChange;
            Points = Math.Max(Points, 0);
            pointText.Text = "Points: " + (Points >= 100 ? "" : " ") + (Points >= 10 ? "" : " ") + Points.ToString();
        }
    }
}
