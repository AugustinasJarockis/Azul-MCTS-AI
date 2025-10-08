using AzulBoardGame.Extensions;
using AzulBoardGame.PlayerBoard;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AzulBoardGame.Players
{
    internal class RandomAI : Player
    {
        private Random rnd = new(DateTime.Now.Microsecond * DateTime.Now.Millisecond);
        private readonly bool _pauseBetweenChoices;

        private Image? waitButton = null;
        private TaskCompletionSource<bool>? waiter = null;
        public RandomAI(
            Canvas mainCanvas,
            ScaleTransform scaleTransform,
            TranslateTransform translateTransform,
            Action notifyAboutCompletion,
            TilePlates tilePlates,
            TileBank tileBank,
            string name,
            Brush nameColour,
            Key keyToFocus,
            double xPos,
            double yPos,
            double size,
            bool pauseBetweenChoices = false
            )
            : base(mainCanvas, scaleTransform, translateTransform, notifyAboutCompletion, tilePlates, tileBank, name, nameColour, keyToFocus, xPos, yPos, size) {

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

            int centerTilesExist = _tilePlates.CenterTileCount != 0 ? 1 : 0;

            var plates = _tilePlates.Plates.Where(p => !p.IsEmpty).ToList();
            int selection = rnd.Next(plates.Count + centerTilesExist);

            if (selection == 0 && centerTilesExist != 0) {
                var centerTileTypes = _tilePlates.CenterTileTypes;
                int tileToSelect = rnd.Next(_tilePlates.CenterTileTypes.Count);
                var selectedType = centerTileTypes[tileToSelect];
                _tilePlates.SelectTiles(selectedType);
            }
            else {
                int tileToSelect = rnd.Next(4);
                var selectedType = plates[selection - centerTilesExist].TileTypes[tileToSelect];
                plates[selection - centerTilesExist].SelectTiles(selectedType);
            }
        }

        public override async Task SelectRow() {
            await WaitToContinue();
            List<TileRow> possibleRows = [];
            
            for (int i = 0; i < tileRows.Count; i++) {
                if (!tileRows[i].IsFull
                    && (tileRows[i].rowTileType == null || tileRows[i].rowTileType == selectedTiles[0].TileType)
                    && !tileGrid.RowHasType(i, selectedTiles[0].TileType))

                    possibleRows.Add(tileRows[i]);
            }

            if (possibleRows.Count > 0) {
                int rowToSelect = rnd.Next(possibleRows.Count);
                TakeSelectedTiles(possibleRows[rowToSelect]);
            }
            else {
                DiscardSelectedTiles();
            }
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
