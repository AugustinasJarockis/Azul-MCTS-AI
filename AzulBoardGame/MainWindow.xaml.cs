using System.Windows;
using System.Windows.Input;

namespace AzulBoardGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var gameManager = new GameManager(MainCanvas, BoardZoom, BoardPos);

            MainCanvas.Focus();
            MainCanvas.KeyDown += (s, e) => {
                if (e.Key == Key.NumPad0)
                    ResetView();
            };
        }

        private void ResetView() {
            BoardPos.X = 0;
            BoardPos.Y = 0;

            BoardZoom.ScaleX = 1;
            BoardZoom.ScaleY = 1;
        }
    }
}