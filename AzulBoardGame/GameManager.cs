using AzulBoardGame.Enums;
using AzulBoardGame.Extensions;
using AzulBoardGame.Players;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AzulBoardGame
{
    internal class GameManager
    {
        private readonly Canvas _mainCanvas;
        private readonly ScaleTransform _scaleTransform;
        private readonly TranslateTransform _translateTransform;

        private Image? waitButton;
        private bool waitBeforeTurnEnd = true;
        private TaskCompletionSource<bool> waiter;

        private List<Player> players = [];
        private TileBank tileBank;
        private TilePlates tilePlates;

        private int plateCount = 9;
        private bool gameStarted = false;

        private TaskCompletionSource<bool> tcs;

        public int CurrentPlayer { get; private set; } = 0;
        public GameManager(Canvas mainCanvas, ScaleTransform scaleTransform, TranslateTransform translateTransform) {
            _mainCanvas = mainCanvas;
            _scaleTransform = scaleTransform;
            _translateTransform = translateTransform;

            tilePlates = new TilePlates(_mainCanvas, _scaleTransform, _translateTransform, this, Key.NumPad5, plateCount);
            tileBank = new TileBank();

            players.Add(new Human(_mainCanvas, _scaleTransform, _translateTransform, NotifyAboutCompletion, tilePlates, tileBank, "RealPlayer", Brushes.Red, Key.NumPad1, 0.18, 0.82, 0.35));
            players.Add(new RandomAI(_mainCanvas, _scaleTransform, _translateTransform, NotifyAboutCompletion, tilePlates, tileBank, "AI1", Brushes.Blue, Key.NumPad3, 0.18, 0.18, 0.35));
            players.Add(new RandomAI(_mainCanvas, _scaleTransform, _translateTransform, NotifyAboutCompletion, tilePlates, tileBank, "AI2", Brushes.Green, Key.NumPad4, 0.82, 0.18, 0.35));
            players.Add(new RandomAI(_mainCanvas, _scaleTransform, _translateTransform, NotifyAboutCompletion, tilePlates, tileBank, "AI3", Brushes.Yellow, Key.NumPad2, 0.82, 0.82, 0.35));

            if (waitBeforeTurnEnd) {
                waitButton = new Image {
                    Source = new BitmapImage(new Uri("Textures/continue.png", UriKind.Relative)),
                    Visibility = Visibility.Hidden
                };

                mainCanvas.Loaded += (s, e) => {
                    mainCanvas.Dispatcher.BeginInvoke(() => {
                        mainCanvas.SetRelativePosCentered(waitButton, 0.5, 0.9, 0.1, 0.3);
                    });
                };

                mainCanvas.Children.Add(waitButton);

                waitButton.MouseDown += (s, a) => waiter?.TrySetResult(true);
                waitButton.MouseEnter += (s, a) => waitButton.Opacity = 0.5;
                waitButton.MouseLeave += (s, a) => waitButton.Opacity = 1.0;
            }

            _mainCanvas.KeyDown += (s, e) => {
                if (e.Key == Key.S && !gameStarted) {
                    gameStarted = true;
                    StartGame();
                }
            };
        }

        public async Task StartGame() {
            while (!players.Any(p => p.HasFinished())) {
                var tileTypes = tileBank.RefreshTiles(plateCount);
                tilePlates.RefreshPlates(tileTypes);

                CurrentPlayer = tilePlates.StartingPlayer;
                while (tilePlates.TotalTileCount > 0) {
                    tcs = new();
                    players[CurrentPlayer].SelectTiles();
                    await tcs.Task;
                    CurrentPlayer = (CurrentPlayer + 1) % 4;
                }

                await WaitToContinue();

                foreach (Player player in players)
                    player.CompleteRound();
            }

            foreach (Player player in players)
                player.CalculateAdditionalPoints();
        }

        public void NotifyAboutCompletion() => tcs?.TrySetResult(true);

        private async Task WaitToContinue() {
            if (waitBeforeTurnEnd) {
                waiter = new();
                waitButton!.Visibility = Visibility.Visible;
                await waiter.Task;
                waitButton.Visibility = Visibility.Hidden;
            }
        }
    }
}
