using AzulBoardGame.Enums;
using AzulBoardGame.Extensions;
using AzulBoardGame.GameState;
using AzulBoardGame.GameTilePlates;
using AzulBoardGame.PlayerBoard;
using AzulBoardGame.PlayerBoard.PlayerTileGrid;
using AzulBoardGame.PlayerBoard.PlayerTileRow;
using AzulBoardGame.PlayerBoard.PointCounter;
using AzulBoardGame.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AzulBoardGame.Players.PlayerBase
{
    internal class PlayerUI : ITileContainer
    {
        public PlayerBoardState State { get; set; }

        private readonly CanvasControls _canvasControls;
        private readonly ITileBank _tileBank;
        protected readonly TilePlates _tilePlates;
        private readonly Key _keyToFocus;

        private readonly bool _pauseBetweenChoices;
        private Image? waitButton = null;
        private TaskCompletionSource<bool>? waiter = null;

        protected readonly Action NotifyAboutCompletion;

        private Canvas playerCanvas;
        protected ProcessingLine processingLine;
        private IPointCounter pointCounter;
        private PlayerNamePanel playerNamePanel;
        protected List<TileRow> tileRows = [];
        protected List<Tile> selectedTiles = [];
        protected TileGrid tileGrid;

        public int Points => State.Points;
        public string Name => playerNamePanel.Name;

        public PlayerUI( //TODO: proper select tiles somehow
            PlayerBoardState playerBoardState,
            CanvasControls canvasControls,
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
            ) {
            State = playerBoardState;

            _canvasControls = canvasControls;
            NotifyAboutCompletion = notifyAboutCompletion;
            _tilePlates = tilePlates;
            _tileBank = tileBank;
            _keyToFocus = keyToFocus;


            playerCanvas = new() { };

            processingLine = new(State.processingLine, playerCanvas, _tileBank, DiscardSelectedTiles);
            pointCounter = new PointCounter(playerCanvas);
            pointCounter.UpdatePoints(State.Points);
            playerNamePanel = new(playerCanvas, name, nameColour);
            tileGrid = new(State.tileGrid);

            _canvasControls.Canvas.Loaded += (s, e) => {
                _canvasControls.Canvas.SetRelativePosCentered(playerCanvas, xPos, yPos, size, size);
            };

            Image playerBoard = new Image {
                Opacity = 0.65,
                Stretch = Stretch.Fill,
                Source = new BitmapImage(new Uri("Textures/playerBoard.png", UriKind.Relative)),
            };

            for (int i = 0; i < 5; i++) {
                tileRows.Add(new(State.tileRows[i], playerCanvas, 0.481, 0.105 + i * 0.142, 0.1475, i * 0.09 + 0.105, i + 1, processingLine, tileBank, TakeSelectedTiles));
            }

            playerCanvas.Loaded += (s, e) => {
                playerCanvas.Dispatcher.BeginInvoke(() => {
                    playerCanvas.SetRelativeDimensions(playerBoard, 1, 1);
                });
            };

            playerCanvas.Children.Add(playerBoard);
            _canvasControls.Canvas.Children.Add(playerCanvas);

            _canvasControls.Canvas.KeyDown += (s, e) => {
                if (e.Key == _keyToFocus)
                    Focus();
            };

            _pauseBetweenChoices = pauseBetweenChoices;

            if (_pauseBetweenChoices) {
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
        }

        public async Task MakeMove((byte plate, TileType type, byte row) moveToMake) {
            await WaitToContinue();
            
            _tilePlates.SetSelectionCallback(ManageSelectedTiles);
            if (moveToMake.plate == 0) {
                _tilePlates.SelectTiles(moveToMake.type);
            }
            else {
                _tilePlates.Plates[moveToMake.plate - 1].SelectTiles(moveToMake.type);
            }

            await WaitToContinue();

            if (moveToMake.row == 5) {
                DiscardSelectedTiles();
            }
            else {
                TakeSelectedTiles(tileRows[moveToMake.row]);
            }
        }

        public void CompleteRound() {
            for (int i = 0; i < tileRows.Count; i++) {
                if (tileRows[i].IsFull) {
                    Tile tileToTransfer = tileRows[i].PrepareForTileTransfer();
                    int pointsGained = tileGrid.AddTile(i, tileToTransfer);
                    pointCounter.UpdatePoints(pointsGained);
                    State.UpdatePoints(pointsGained);
                }
            }
            int pointsLost = processingLine.Clear();
            pointCounter.UpdatePoints(-pointsLost);
            State.UpdatePoints(-pointsLost);
        }

        public void ManageSelectedTiles(List<Tile> tiles) {
            State.ManageSelectedTiles([.. tiles.Select(t => t.TileType)], _tileBank);
            for (int i = 0; i < tiles.Count; i++) {
                tiles[i].Move(playerCanvas, this, 0.05 + i * 0.075, 0.8, 0.1);
            }

            if (tiles[^1].TileType == TileType.First) {
                processingLine.AddTile(tiles[^1]);
                tiles.Remove(tiles[^1]);
            }

            selectedTiles = tiles;

            _tilePlates.DisableUserInput();
            _tilePlates.ClearSelectionCallback();
        }

        public void TakeSelectedTiles(TileRow tileRow) {
            tileRow.AddTiles(selectedTiles);

            RemoveSelectedTiles();
        }

        public void DiscardSelectedTiles() {
            processingLine.AddTiles(selectedTiles);
            processingLine.State.AddTiles([..selectedTiles.Select(t => t.TileType)], _tileBank);
            RemoveSelectedTiles();
        }

        protected void RemoveSelectedTiles() {
            selectedTiles.Clear();
            State.selectedTiles.Clear();
            foreach (var row in tileRows) {
                row.StopMouseInput();
            }

            processingLine.StopMouseInput();

            EndPlayersTurn();
            //NotifyAboutCompletion(); //TODO: think about this
        }

        public void CalculateAdditionalPoints() {
            int totalPointChange = 0;
            for (int i = 0; i < 5; i++) {
                if (tileGrid.RowIsFull(i))
                    totalPointChange += 2;

                if (tileGrid.CollumnIsFull(i))
                    totalPointChange += 7;

                if (tileGrid.TypeIsComplete((TileType)(i + 1)))
                    totalPointChange += 10;
            }

            pointCounter.UpdatePoints(totalPointChange);
            State.UpdatePoints(totalPointChange);
        }

        public void SetPlayersTurn() => playerNamePanel.ShowPlayerTurn();
        public void EndPlayersTurn() => playerNamePanel.HidePlayerTurn();

        public void SelectTiles(TileType type) { }
        public void HighlightTiles(TileType type) { }
        public void UnhighlightTiles(TileType type) { }

        private async Task WaitToContinue() {
            if (_pauseBetweenChoices) {
                waiter = new();
                waitButton!.Visibility = Visibility.Visible;
                await waiter.Task;
                waitButton.Visibility = Visibility.Hidden;
            }
        }
        public void Focus() {
            _canvasControls.TranslateTransform.X = -Canvas.GetLeft(playerCanvas);
            _canvasControls.TranslateTransform.Y = -Canvas.GetTop(playerCanvas);

            _canvasControls.ScaleTransform.ScaleX = _canvasControls.Canvas.ActualWidth / playerCanvas.ActualWidth;
            _canvasControls.ScaleTransform.ScaleY = _canvasControls.ScaleTransform.ScaleX;
        }
    }
}
