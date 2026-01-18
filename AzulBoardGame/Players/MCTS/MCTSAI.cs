using AzulBoardGame.Enums;
using AzulBoardGame.Extensions;
using AzulBoardGame.GameTilePlates;
using AzulBoardGame.Players.MCTS.StateEvaluators;
using AzulBoardGame.Players.PlayerBase;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AzulBoardGame.Players.MCTS
{
    internal class MCTSAI : Player
    {
        public int timeAllotedMs = 500;

        private readonly bool _pauseBetweenChoices;
        private readonly GameManager _gameManager;
        private readonly IStateEvaluator _stateEvaluator;

        private (byte plate, TileType type, byte row) move;
        private GameState? gameState = null;

        private Image? waitButton = null;
        private TaskCompletionSource<bool>? waiter = null;

        public MCTSAI(
            IStateEvaluator evaluator,
            GameManager gameManager,
            Canvas mainCanvas,
            ScaleTransform scaleTransform,
            TranslateTransform translateTransform,
            Action notifyAboutCompletion,
            TilePlates tilePlates,
            ITileBank tileBank,
            string name,
            Brush nameColour,
            Key keyToFocus,
            double xPos,
            double yPos,
            double size,
            bool pauseBetweenChoices = false
            )
            : base(mainCanvas, scaleTransform, translateTransform, notifyAboutCompletion, tilePlates, tileBank, name, nameColour, keyToFocus, xPos, yPos, size) {

            _stateEvaluator = evaluator;
            _gameManager = gameManager;
            _pauseBetweenChoices = pauseBetweenChoices;

            if (_pauseBetweenChoices) {

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
        }

        public override async Task SelectTiles() {
            await WaitToContinue();
            SetPlayersTurn();
            _tilePlates.SetSelectionCallback(ManageSelectedTiles);

            // Logic
            if (gameState == null) {
                gameState = _gameManager.GetState(_gameManager.CurrentPlayer, _stateEvaluator);
            }
            else {
                gameState = gameState.GetSyncWithManager(_gameManager.PlayerCount, _gameManager);
            }

            var timer = Stopwatch.StartNew();
            while (timer.ElapsedMilliseconds < timeAllotedMs) {
                gameState.PlayOut(true);
            }
            timer.Stop();

            Console.WriteLine("Nodes visited: " + gameState.EndsReached);
            move = gameState.GetBestMove();

            MoveMade = move;
            if (move.plate == 0) {
                _tilePlates.SelectTiles(move.type);
            }
            else {
                _tilePlates.Plates[move.plate - 1].SelectTiles(move.type);
            }
        }

        public override async Task SelectRow() {
            await WaitToContinue();

            if (move.row == 5)
                DiscardSelectedTiles();
            else
                TakeSelectedTiles(tileRows[move.row]);
        }

        protected override void RemoveSelectedTiles() {
            selectedTiles.Clear();
            EndPlayersTurn();
            NotifyAboutCompletion();
        }

        private async Task WaitToContinue() {
            if (_pauseBetweenChoices) {
                waiter = new();
                waitButton!.Visibility = Visibility.Visible;
                await waiter.Task;
                waitButton.Visibility = Visibility.Hidden;
            }
        }
    }
}
