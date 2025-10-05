using AzulBoardGame.Enums;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AzulBoardGame
{
    internal class GameManager
    {
        private readonly Canvas _mainCanvas;
        private readonly ScaleTransform _scaleTransform;
        private readonly TranslateTransform _translateTransform;

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

            players.Add(new(_mainCanvas, _scaleTransform, _translateTransform, NotifyAboutCompletion, tilePlates, tileBank, "Test1", Brushes.Red, PlayerType.RandomAI, Key.NumPad1, 0.18, 0.82, 0.35));
            players.Add(new(_mainCanvas, _scaleTransform, _translateTransform, NotifyAboutCompletion, tilePlates, tileBank, "Test3", Brushes.Blue, PlayerType.RandomAI, Key.NumPad3, 0.18, 0.18, 0.35));
            players.Add(new(_mainCanvas, _scaleTransform, _translateTransform, NotifyAboutCompletion, tilePlates, tileBank, "Test4", Brushes.Green, PlayerType.RandomAI, Key.NumPad4, 0.82, 0.18, 0.35));
            players.Add(new(_mainCanvas, _scaleTransform, _translateTransform, NotifyAboutCompletion, tilePlates, tileBank, "Test2", Brushes.Yellow, PlayerType.RandomAI, Key.NumPad2, 0.82, 0.82, 0.35));

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
                foreach (Player player in players)
                    player.CompleteRound();
            }

            foreach (Player player in players)
                player.CalculateAdditionalPoints();
        }

        public void NotifyAboutCompletion() => tcs?.TrySetResult(true);
    }
}
