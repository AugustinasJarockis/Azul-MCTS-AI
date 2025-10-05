using AzulBoardGame.Enums;
using AzulBoardGame.Extensions;
using AzulBoardGame.PlayerBoard;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AzulBoardGame
{
    internal class Player : ITileContainer
    {
        private readonly Canvas _mainCanvas;
        private readonly ScaleTransform _scaleTransform;
        private readonly TranslateTransform _translateTransform;
        private readonly TilePlates _tilePlates;
        private readonly TileBank _tileBank;
        private readonly Key _keyToFocus;

        private readonly Action NotifyAboutCompletion;

        private Canvas playerCanvas;
        private ProcessingLine processingLine;
        private PointCounter pointCounter;
        private PlayerNamePanel playerNamePanel;
        private List<TileRow> tileRows = [];
        private List<Tile> selectedTiles = [];
        private TileGrid tileGrid;

        public Player(
            Canvas mainCanvas, 
            ScaleTransform scaleTransform, 
            TranslateTransform translateTransform,
            Action nofityAboutCompletion,
            TilePlates tilePlates,
            TileBank tileBank,
            string name,
            Brush nameColour,
            PlayerType playerType,
            Key keyToFocus,
            double xPos, 
            double yPos, 
            double size
            ) {
            _mainCanvas = mainCanvas;
            _scaleTransform = scaleTransform;
            _translateTransform = translateTransform;
            NotifyAboutCompletion = nofityAboutCompletion;
            _tilePlates = tilePlates;
            _tileBank = tileBank;
            _keyToFocus = keyToFocus;


            playerCanvas = new() {
                Name = name + "Canvas"
            };

            processingLine = new(playerCanvas, _tileBank, DiscardSelectedTiles);
            pointCounter = new(playerCanvas);
            playerNamePanel = new(playerCanvas, name, nameColour);
            tileGrid = new();

            _mainCanvas.Loaded += (s, e) => {
                _mainCanvas.SetRelativePosCentered(playerCanvas, xPos, yPos, size, size);
            };

            Image playerBoard = new Image {
                Name = "Player" + name,
                Opacity = 0.65,
                Stretch = Stretch.Fill,
                Source = new BitmapImage(new Uri("Textures/playerBoard.png", UriKind.Relative)),
            };

            for (int i = 0; i < 5; i++) {
                tileRows.Add(new(playerCanvas, 0.481, 0.105 + i * 0.142, 0.1475, i * 0.09 + 0.105, i + 1, processingLine, tileBank, TakeSelectedTiles));
            }

            playerCanvas.Loaded += (s, e) => {
                playerCanvas.Dispatcher.BeginInvoke(() => {
                    playerCanvas.SetRelativeDimensions(playerBoard, 1, 1);
                });
            };

            playerCanvas.Children.Add(playerBoard);
            _mainCanvas.Children.Add(playerCanvas);

            _mainCanvas.KeyDown += (s, e) => {
                if (e.Key == _keyToFocus)
                    Focus();
            };
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
        }

        public bool HasFinished() {
            for (int i = 0; i < 5; i++)
                if (tileGrid.RowIsFull(i))
                    return true;

            return false;
        }

        public void CompleteRound() {
            for (int i = 0; i < tileRows.Count; i++) {
                if (tileRows[i].IsFull) {
                    Tile tileToTransfer = tileRows[i].PrepareForTileTransfer();
                    int pointsGained = tileGrid.AddTile(i, tileToTransfer);
                    pointCounter.UpdatePoints(pointsGained);
                }
            }
            int pointsLost = processingLine.Clear();
            pointCounter.UpdatePoints(-pointsLost);
        }

        public void SelectTiles() {
            SetPlayersTurn();
            _tilePlates.EnableUserInput();
            _tilePlates.SetSelectionCallback(ManageSelectedTiles);
        }
        
        public void ManageSelectedTiles(List<Tile> tiles) {
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

            for (int  i = 0; i < tileRows.Count; i++) {
                if (!tileRows[i].IsFull 
                    && (tileRows[i].rowTileType == null || tileRows[i].rowTileType == selectedTiles[0].TileType)
                    && !tileGrid.RowHasType(i, selectedTiles[0].TileType))
                    
                    tileRows[i].StartMouseInput();
            }

            processingLine.StartMouseInput();
        }

        public void TakeSelectedTiles(TileRow tileRow) {
            tileRow.AddTiles(selectedTiles);
            RemoveSelectedTiles();
        }

        public void DiscardSelectedTiles() {
            processingLine.AddTiles(selectedTiles);
            RemoveSelectedTiles();
        }

        private void RemoveSelectedTiles() {
            selectedTiles.Clear();
            foreach (var row in tileRows) {
                row.StopMouseInput();
            }

            processingLine.StopMouseInput();

            EndPlayersTurn();
            NotifyAboutCompletion();
        }

        public void SetPlayersTurn() => playerNamePanel.ShowPlayerTurn();
        public void EndPlayersTurn() => playerNamePanel.HidePlayerTurn();

        public void SelectTiles(TileType type) {}
        public void HighlightTiles(TileType type) {}
        public void UnhighlightTiles(TileType type) {}
        
        public void Focus() {
            _translateTransform.X = -Canvas.GetLeft(playerCanvas);
            _translateTransform.Y = -Canvas.GetTop(playerCanvas);
            
            _scaleTransform.ScaleX = _mainCanvas.ActualWidth / playerCanvas.ActualWidth;
            _scaleTransform.ScaleY = _scaleTransform.ScaleX;
        }
    }
}
