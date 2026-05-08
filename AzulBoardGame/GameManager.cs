using AzulAIAndGameState.Players.MCTS_CNN;
using AzulAIAndGameState.Players.MCTS_NN;
using AzulAIAndGameState.Players.MCTS_NN.Networks;
using AzulAIAndGameState.Players.MiniMax;
using AzulBoardGame.Enums;
using AzulBoardGame.Extensions;
using AzulBoardGame.GameState;
using AzulBoardGame.GameTilePlates;
using AzulBoardGame.Players;
using AzulBoardGame.Players.MCTS;
using AzulBoardGame.Players.MCTS.MCTSVariants;
using AzulBoardGame.Players.PlayerBase;
using AzulBoardGame.Utilities;
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
        private readonly CanvasControls _canvasControls;

        private VictoryPopup victoryPopup;
        private Image? waitButton;
        private bool waitBeforeTurnEnd = false;
        private TaskCompletionSource<bool> waiter;

        public GeneralGameState gameState;
        private List<Player> players = [];
        private TileBank tileBank;
        private TilePlates tilePlates;

        private bool gameStarted = false;

        private Image startButton = null;

        private TaskCompletionSource<bool> tcs;

        public bool runTests = false;
        private int playerCount = 2;
        public int PlayerCount => players.Count;
        public int PlateCount => gameState.TilePlatesState.Plates.Count;
        public (byte plate, TileType type, byte row)?[] recentMoves = [null, null, null, null];


        //Create Player AIs
        //public IPlayerAI player1 = new HeuristicAI();
        //public IPlayerAI player1 = new MCTSnPolicy("Models/fullmodel10.v3.nn", trainingOn: false);
        //public IPlayerAI player1 = new MCTSAIScoreDiffAvg();
        public IPlayerAI player1 = new MinimaxAI();
        //public IPlayerAI player1 = new MCTSnNNAI(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"), new SmallConvValueNetwork("Models/smallconvvaluemodel71.v0.nn"), trainingOn: false);
        //public IPlayerAI player1 = new MCTSnNNAI(new SmallConvPolicyNetwork("Models/RL/RLPolicy.v0.nn"), new SmallConvValueNetwork("Models/RL/RLValue.v0.nn"), trainingOn: false);
        //public IPlayerAI player1 = new MCTSnPolicy(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"), trainingOn: false);
        //var player1 = new MCTSnNNAI("Models/nmodel31.nn", trainingOn: true);
        //public IPlayerAI player2 = new MCTSnNNAI("Models/hmodel48.nn", trainingOn: true);
        //public IPlayerAI player1 = new MCTSnNNAI("Models/fullmodel10.v3.nn", "Models/valuemodel58.v0.nn", trainingOn: false, timeAllotedMs: 10000);
        //public IPlayerAI player2 = new MCTSnNNAI(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"), new SmallConvValueNetwork("Models/smallconvvaluemodel71.v0.nn"), trainingOn: false);
        //public IPlayerAI player2 = new MCTSnNNAI("Models/fullmodel10.v3.nn", "Models/valuemodel21.v1.nn", trainingOn: false);
        //public IPlayerAI player2 = new MCTSnNNAI("Models/fullmodel10.v3.nn", "Models/RLValue.nn", trainingOn: false);
        //public IPlayerAI player2 = new MCTSnNNAI("Models/fullmodel10.v3.nn", "Models/valuemodel21.v1.nn", trainingOn: false);
        //public IPlayerAI player2 = new MCTSnNNAI(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"), new SmallConvValueNetwork("Models/smallconvvaluemodel71.v0.nn"), trainingOn: false);
        //public IPlayerAI player2 = new MCTSnNNAI(new SmallConvPolicyNetwork("Models/RL/RLPolicy.v0.nn"), new SmallConvValueNetwork("Models/RL/RLValue.v0.nn"), trainingOn: false);
        public IPlayerAI player2 = new MCTSnCustomEval(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"), trainingOn: false);
        //public IPlayerAI player2 = new MCTSnPolicy("Models/fullmodel10.v3.nn", trainingOn: false);
        //public IPlayerAI player2 = new PolicyNetworkAI(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"));
        //public IPlayerAI player2 = new MinimaxAI();
        //public IPlayerAI player2 = new HeuristicAI();
        public GameManager(Canvas mainCanvas, ScaleTransform scaleTransform, TranslateTransform translateTransform) {
            _canvasControls = new (){
                Canvas = mainCanvas,
                ScaleTransform = scaleTransform,
                TranslateTransform = translateTransform
            };

            CreateGameBoardObjects();

            _canvasControls.Canvas.KeyDown += (s, e) => {
                if (e.Key == Key.S && !gameStarted) {
                    StartGame();
                }
            };

            //Test(100, "PossiblyWorkingRL2.csv");
            if (runTests) {
                Test(100, "PolicyVsRandom.csv");
            }
        }

        private void CreateGameBoardObjects() {
            gameState = new(playerCount);
            victoryPopup = new VictoryPopup(_canvasControls.Canvas, ResetGame);
            tilePlates = new TilePlates(_canvasControls, this, Key.NumPad5, gameState.TilePlatesState);
            tileBank = new TileBank();

            //Agents to be tested
            players.Add(new(gameState.PlayerBoardStates[0], player1, _canvasControls, NotifyAboutCompletion, tilePlates, tileBank, "Petras", Brushes.Red, Key.NumPad1, 0.18, 0.82, 0.35));
            players.Add(new(gameState.PlayerBoardStates[1], player2, _canvasControls, NotifyAboutCompletion, tilePlates, tileBank, "Jonas", Brushes.Blue, Key.NumPad2, 0.18, 0.18, 0.35));
            
            if (waitBeforeTurnEnd) {
                waitButton = new Image {
                    Source = new BitmapImage(new Uri("Textures/continue.png", UriKind.Relative)),
                    Visibility = Visibility.Hidden
                };

                _canvasControls.Canvas.Loaded += (s, e) => {
                    _canvasControls.Canvas.Dispatcher.BeginInvoke(() => {
                        _canvasControls.Canvas.SetRelativePosCentered(waitButton, 0.5, 0.9, 0.1, 0.3);
                    });
                };

                _canvasControls.Canvas.Children.Add(waitButton);

                waitButton.MouseDown += (s, a) => waiter?.TrySetResult(true);
                waitButton.MouseEnter += (s, a) => waitButton.Opacity = 0.5;
                waitButton.MouseLeave += (s, a) => waitButton.Opacity = 1.0;
            }

            startButton = new Image {
                Source = new BitmapImage(new Uri("Textures/start.png", UriKind.Relative)),
                Visibility = Visibility.Visible
            };

            _canvasControls.Canvas.Loaded += (s, e) => {
                _canvasControls.Canvas.Dispatcher.BeginInvoke(() => {
                    _canvasControls.Canvas.SetRelativePosCentered(startButton, 0.5, 0.9, 0.1, 0.3);
                });
            };

            _canvasControls.Canvas.Children.Add(startButton);

            startButton.MouseDown += (s, a) => { if (!gameStarted) StartGame(); };
            startButton.MouseEnter += (s, a) => startButton.Opacity = 0.5;
            startButton.MouseLeave += (s, a) => startButton.Opacity = 1.0;
        }

        public void StartGame() {
            gameStarted = true;
            startButton.Visibility = Visibility.Hidden;

            RunMatch();
        }

        public void ResetGame() {
            gameStarted = false;
            players.Clear();
            _canvasControls.Canvas.Children.Clear();
            var mainWindow = Application.Current.MainWindow;
            mainWindow.Content = null;
            mainWindow.Content = _canvasControls.Canvas;
            CreateGameBoardObjects();
        }

        private async Task Test(int count, string filename, int player1TimeMs = -1) {
            File.Create(filename);

            for (int i = 0; i < count; i++) {
                int startingPlayer = i / ((count + players.Count - 1) / players.Count);
                gameState.NextRoundStartingPlayer = startingPlayer;
                gameState.CurrentPlayer = startingPlayer;
                startButton.Visibility = Visibility.Hidden;
                gameStarted = true;
                if (player1TimeMs > 0) {
                    ((MCTSAI)players[0].PlayerAI).timeAllotedMs = player1TimeMs;
                }
                await Task.Delay(2000);
                await PlayGame();
                WriteResults(filename, startingPlayer);
                //((MCTSnNNAI)players[1].PlayerAI).SaveModel("Models/RLPolicyNoChange.nn", "Models/RLValue2.nn");
                ResetGame();
            }
        }

        private void WriteResults(string filename, int startingPlayer) {
            string textToAppend = "";
            string delimiter = "; ";
            textToAppend += players.Count + delimiter;
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
            //((MCTSnNNAI)players[1].PlayerAI).SaveModel("Models/RLPolicyNoChange.nn", "Models/RLValue2.nn");

            victoryPopup.Show(winningPlayer.Name, winningPlayer.Points);
            gameStarted = false;
        }

        private async Task PlayGame() {
            while (!players.Any(p => p.HasFinished())) {
                var tileTypes = tileBank.RefreshTiles(PlateCount);
                tilePlates.RefreshPlates(tileTypes);

                while (tilePlates.TotalTileCount > 0) {
                    tcs = new();
                    await players[gameState.CurrentPlayer].MakeMove(gameState);
                    await tcs.Task;
                    _canvasControls.Canvas.InvalidateVisual();
                    await Dispatcher.Yield(DispatcherPriority.Render);
                    recentMoves[gameState.CurrentPlayer] = gameState.PlayerBoardStates[gameState.CurrentPlayer].MoveMade;
                    gameState.CurrentPlayer = (gameState.CurrentPlayer + 1) % players.Count; //TODO: investigate if potentially current player is updated thrice
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
