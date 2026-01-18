using AzulBoardGame.Enums;
using AzulBoardGame.Extensions;
using AzulBoardGame.GameTilePlates;
using AzulBoardGame.Players;
using AzulBoardGame.Players.MCTS;
using AzulBoardGame.Players.MCTS.MCTSVariants;
using AzulBoardGame.Players.MCTS.StateEvaluators;
using AzulBoardGame.Players.PlayerBase;
using System.IO;
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
                Test(900, "RepeatedBestAgentTests.csv");
                //RunTests();
            }
        }

        private void CreateGameBoardObjects() {
            victoryPopup = new VictoryPopup(_mainCanvas, ResetGame);
            tilePlates = new TilePlates(_mainCanvas, _scaleTransform, _translateTransform, this, Key.NumPad5, plateCount);
            tileBank = new TileBank();

            //Agents to be tested
            players.Add(new MCTSAIScoreDiffAvg(this, _mainCanvas, _scaleTransform, _translateTransform, NotifyAboutCompletion, tilePlates, tileBank, "ScoreDiffAvg", Brushes.Red, Key.NumPad1, 0.18, 0.82, 0.35));
            players.Add(new MCTSAIScoreTotalAvg(this, _mainCanvas, _scaleTransform, _translateTransform, NotifyAboutCompletion, tilePlates, tileBank, "ScoreTotalAvg", Brushes.Blue, Key.NumPad2, 0.18, 0.18, 0.35));
            //players.Add(new HeuristicAI(_mainCanvas, _scaleTransform, _translateTransform, NotifyAboutCompletion, tilePlates, tileBank, "Heuristic", Brushes.Blue, Key.NumPad2, 0.18, 0.18, 0.35));

            //players.Add(new Human(_mainCanvas, _scaleTransform, _translateTransform, NotifyAboutCompletion, tilePlates, tileBank, "Žaidėjas 1", Brushes.Red, Key.NumPad1, 0.18, 0.82, 0.35));
            //players.Add(new Human(_mainCanvas, _scaleTransform, _translateTransform, NotifyAboutCompletion, tilePlates, tileBank, "Žaidėjas 2", Brushes.Blue, Key.NumPad3, 0.18, 0.18, 0.35));
            //players.Add(new Human(_mainCanvas, _scaleTransform, _translateTransform, NotifyAboutCompletion, tilePlates, tileBank, "Žaidėjas 3", Brushes.Green, Key.NumPad4, 0.82, 0.18, 0.35));
            //players.Add(new Human(_mainCanvas, _scaleTransform, _translateTransform, NotifyAboutCompletion, tilePlates, tileBank, "Žaidėjas 4", Brushes.Yellow, Key.NumPad2, 0.82, 0.82, 0.35));

            // Heuristic vs Random
            //players.Add(new HeuristicAI(_mainCanvas, _scaleTransform, _translateTransform, NotifyAboutCompletion, tilePlates, tileBank, "Heuristic", Brushes.Green, Key.NumPad1, 0.82, 0.18, 0.35));
            //players.Add(new RandomAI(_mainCanvas, _scaleTransform, _translateTransform, NotifyAboutCompletion, tilePlates, tileBank, "Random", Brushes.Yellow, Key.NumPad2, 0.82, 0.82, 0.35));

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

        public GameState GetState(int playerOfInterest, IStateEvaluator stateEvaluator) {
            // TODO: determinizmas
            return new (tileBank.GetDeterministicCopy(), tilePlates, players, CurrentPlayer, playerOfInterest, stateEvaluator);
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

        private async Task RunTests() {
            //0.001s
            await Test(100, "HeuristicTime0.001s.csv", 1);
            //0.002s
            await Test(100, "HeuristicTime0.002s.csv", 2);
            //0.005s
            await Test(100, "HeuristicTime0.005s.csv", 5);
            //0.01s
            await Test(100, "HeuristicTime0.01s.csv", 10);
            //0.025s
            await Test(100, "HeuristicTime0.025s.csv", 25);
            //0.05s
            await Test(100, "HeuristicTime0.05s.csv", 50);
            //0.1s
            await Test(100, "HeuristicTime0.1s.csv", 100);
            //0.2s
            await Test(100, "HeuristicTime0.2s.csv", 200);
            //0.3s
            await Test(100, "HeuristicTime0.3s.csv", 300);
            //0.4s
            await Test(100, "HeuristicTime0.4s.csv", 400);
            //0.5s
            await Test(100, "HeuristicTime0.5s.csv", 500);
            //0.6s
            await Test(100, "HeuristicTime0.6s.csv", 600);
            //0.7s
            await Test(100, "HeuristicTime0.7s.csv", 700);
            //0.8s
            await Test(100, "HeuristicTime0.8s.csv", 800);
            //0.9s
            await Test(100, "HeuristicTime0.9s.csv", 900);
            //1s
            await Test(100, "HeuristicTime1s.csv", 1000);
        }

        private async Task Test(int count, string filename, int player1TimeMs = -1) {
            File.Create(filename);

            for (int i = 0; i < count; i++) {
                int startingPlayer = i / ((count + players.Count - 1) / players.Count); 
                tilePlates.StartingPlayer = startingPlayer;
                startButton.Visibility = Visibility.Hidden;
                gameStarted = true;
                if (player1TimeMs > 0) {
                    ((MCTSAI)players[0]).timeAllotedMs = player1TimeMs;
                }
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
