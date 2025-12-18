using AzulBoardGame.Enums;
using AzulBoardGame.Extensions;
using AzulBoardGame.GameTilePlates;
using AzulBoardGame.Players;
using AzulBoardGame.Players.MCTS;
using AzulBoardGame.Players.PlayerBase;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AzulBoardGame
{
    internal class GameManager
    {
        private readonly Canvas _mainCanvas;
        private readonly ScaleTransform _scaleTransform;
        private readonly TranslateTransform _translateTransform;

        private VictoryPopup victoryPopup;
        private Image? waitButton;
        private bool waitBeforeTurnEnd = false;
        private TaskCompletionSource<bool> waiter;

        private List<Player> players = [];
        private TileBank tileBank;
        private TilePlates tilePlates;

        private int plateCount = 5;
        private bool gameStarted = false;

        private Image startButton = null;

        private TaskCompletionSource<bool> tcs;

        public bool runTests = true;

        public int CurrentPlayer { get; private set; } = 0;
        public int PlayerCount => players.Count;
        public (byte plate, TileType type, byte row)?[] recentMoves = [null, null, null, null]; 
        public GameManager(Canvas mainCanvas, ScaleTransform scaleTransform, TranslateTransform translateTransform) {
            _mainCanvas = mainCanvas;
            _scaleTransform = scaleTransform;
            _translateTransform = translateTransform;

            CreateGameBoardObjects();

            _mainCanvas.KeyDown += (s, e) => {
                if (e.Key == Key.S && !gameStarted) {
                    StartGame();
                }
            };

            if (runTests) {
                Test(100, "test.csv");
            }
        }

        private void CreateGameBoardObjects() {
            victoryPopup = new VictoryPopup(_mainCanvas, ResetGame);
            tilePlates = new TilePlates(_mainCanvas, _scaleTransform, _translateTransform, this, Key.NumPad5, plateCount);
            tileBank = new TileBank();

            players.Add(new MCTSAI(this, _mainCanvas, _scaleTransform, _translateTransform, NotifyAboutCompletion, tilePlates, tileBank, "MCTSAI", Brushes.Red, Key.NumPad1, 0.18, 0.82, 0.35));
            players.Add(new HeuristicAI(_mainCanvas, _scaleTransform, _translateTransform, NotifyAboutCompletion, tilePlates, tileBank, "Heuristic AI", Brushes.Red, Key.NumPad1, 0.18, 0.18, 0.35));
            //players.Add(new MCTSAI(this, _mainCanvas, _scaleTransform, _translateTransform, NotifyAboutCompletion, tilePlates, tileBank, "HeuristicAI", Brushes.Blue, Key.NumPad3, 0.18, 0.18, 0.35, true));
            //players.Add(new MCTSAI(this, _mainCanvas, _scaleTransform, _translateTransform, NotifyAboutCompletion, tilePlates, tileBank, "AI2", Brushes.Green, Key.NumPad4, 0.82, 0.18, 0.35, true));
            //players.Add(new MCTSAI(this, _mainCanvas, _scaleTransform, _translateTransform, NotifyAboutCompletion, tilePlates, tileBank, "AI3", Brushes.Yellow, Key.NumPad2, 0.82, 0.82, 0.35, true));

            //players.Add(new Human(_mainCanvas, _scaleTransform, _translateTransform, NotifyAboutCompletion, tilePlates, tileBank, "RealPlayer", Brushes.Red, Key.NumPad1, 0.18, 0.82, 0.35));
            //players.Add(new Human(_mainCanvas, _scaleTransform, _translateTransform, NotifyAboutCompletion, tilePlates, tileBank, "HeuristicAI", Brushes.Blue, Key.NumPad3, 0.18, 0.18, 0.35));
            //players.Add(new Human(_mainCanvas, _scaleTransform, _translateTransform, NotifyAboutCompletion, tilePlates, tileBank, "AI2", Brushes.Green, Key.NumPad4, 0.82, 0.18, 0.35));
            //players.Add(new Human(_mainCanvas, _scaleTransform, _translateTransform, NotifyAboutCompletion, tilePlates, tileBank, "AI3", Brushes.Yellow, Key.NumPad2, 0.82, 0.82, 0.35));

            if (waitBeforeTurnEnd) {
                waitButton = new Image {
                    Source = new BitmapImage(new Uri("Textures/continue.png", UriKind.Relative)),
                    Visibility = Visibility.Hidden
                };

                _mainCanvas.Loaded += (s, e) => {
                    _mainCanvas.Dispatcher.BeginInvoke(() => {
                        _mainCanvas.SetRelativePosCentered(waitButton, 0.5, 0.9, 0.1, 0.3);
                    });
                };

                _mainCanvas.Children.Add(waitButton);

                waitButton.MouseDown += (s, a) => waiter?.TrySetResult(true);
                waitButton.MouseEnter += (s, a) => waitButton.Opacity = 0.5;
                waitButton.MouseLeave += (s, a) => waitButton.Opacity = 1.0;
            }

            startButton = new Image {
                Source = new BitmapImage(new Uri("Textures/start.png", UriKind.Relative)),
                Visibility = Visibility.Visible
            };

            _mainCanvas.Loaded += (s, e) => {
                _mainCanvas.Dispatcher.BeginInvoke(() => {
                    _mainCanvas.SetRelativePosCentered(startButton, 0.5, 0.9, 0.1, 0.3);
                });
            };

            _mainCanvas.Children.Add(startButton);

            startButton.MouseDown += (s, a) => { if (!gameStarted) StartGame(); };
            startButton.MouseEnter += (s, a) => startButton.Opacity = 0.5;
            startButton.MouseLeave += (s, a) => startButton.Opacity = 1.0;
        }

        public GameState GetState(int playerOfInterest) {
            // TODO: determinizmas
            return new (tileBank.GetDeterministicCopy(), tilePlates, players, CurrentPlayer, playerOfInterest);
        }

        public void StartGame() {
            gameStarted = true;
            startButton.Visibility = Visibility.Hidden;

            RunMatch();
        }

        public void ResetGame() {
            gameStarted = false;
            players.Clear();
            _mainCanvas.Children.Clear();
            var mainWindow = Application.Current.MainWindow;
            mainWindow.Content = null;
            mainWindow.Content = _mainCanvas;
            CreateGameBoardObjects();
        }

        private async Task Test(int count, string filename) {
            File.Create(filename);

            for (int i = 0; i < count; i++) {
                int startingPlayer = i / ((count + players.Count - 1) / players.Count); 
                tilePlates.StartingPlayer = startingPlayer;
                startButton.Visibility = Visibility.Hidden;
                gameStarted = true;
                await Task.Delay(2000);
                await PlayGame();
                WriteResults(filename, startingPlayer);
                ResetGame();
            }
        } 

        private void WriteResults(string filename, int startingPlayer) {
            string textToAppend = "";
            string delimiter = "; ";
            textToAppend += players.Count  + delimiter;
            textToAppend += startingPlayer + delimiter;
            foreach (var player in players) {
                textToAppend += player.Name + delimiter;
                textToAppend += player.ToString() + delimiter;
                textToAppend += player.Points + delimiter;
            }

            textToAppend += players.IndexOf(players.MaxBy(p => p.Points)).ToString() + '\n';

            File.AppendAllText(filename, textToAppend);
        }

        private async Task RunMatch() {
            await PlayGame();
            var winningPlayer = players.First(p => p.Points == players.Max(p => p.Points));

            victoryPopup.Show(winningPlayer.Name, winningPlayer.Points);
            gameStarted = false;
        }

        private async Task PlayGame() {
            while (!players.Any(p => p.HasFinished())) {
                var tileTypes = tileBank.RefreshTiles(plateCount);
                tilePlates.RefreshPlates(tileTypes);

                CurrentPlayer = tilePlates.StartingPlayer;
                while (tilePlates.TotalTileCount > 0) {
                    tcs = new();
                    players[CurrentPlayer].SelectTiles();
                    await tcs.Task;
                    _mainCanvas.InvalidateVisual();
                    await Dispatcher.Yield(DispatcherPriority.Render);
                    recentMoves[CurrentPlayer] = players[CurrentPlayer].MoveMade;
                    CurrentPlayer = (CurrentPlayer + 1) % players.Count;
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
