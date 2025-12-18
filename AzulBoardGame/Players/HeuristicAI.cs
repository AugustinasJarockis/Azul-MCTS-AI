using AzulBoardGame.Enums;
using AzulBoardGame.Extensions;
using AzulBoardGame.GamePlates;
using AzulBoardGame.GameTilePlates;
using AzulBoardGame.PlayerBoard.PlayerTileRow;
using AzulBoardGame.Players.PlayerBase;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AzulBoardGame.Players
{
    internal class HeuristicAI : Player
    {
        private readonly bool _pauseBetweenChoices;

        private Image? waitButton = null;
        private TaskCompletionSource<bool>? waiter = null;
        public HeuristicAI(
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

        private List<IPlate> platesWithCount(List<IPlate> plates, TileType type, int count) => [.. plates.Where(p => p.TileTypes.Count(t => t == type) == count)];
        private int tilesInCenter(TileType type) => _tilePlates.CenterTileTypes.Count(t => t == type);

        private bool CanReasonablyFit(TileType type, int count) {
            for (int rowNr = Math.Max(0, count - 1); rowNr < 5; rowNr++) {
                if (CanReasonablyFitIntoRow(type, count, rowNr >= 2 ? 1 : 0, rowNr))
                    return true;
            }
            return false;
        }

        private bool CanReasonablyFitIntoRow(TileType type, int count, int degreeOfFreedom, ITileRow row) => 
            (row.rowTileType == type || row.IsEmpty) 
            && row.FreeSlotCount >= count - degreeOfFreedom;
        private bool CanReasonablyFitIntoRow(TileType type, int count, int degreeOfFreedom, int rowNr) => 
            (tileRows[rowNr].rowTileType == type || tileRows[rowNr].IsEmpty) 
            && tileRows[rowNr].FreeSlotCount >= count - degreeOfFreedom
            && !tileGrid.RowHasType(rowNr, type);
        private bool CanFitIntoRow(TileType type, int count, ITileRow row) => 
            (row.rowTileType == type || row.IsEmpty) 
            && row.FreeSlotCount >= count;

        public override async Task SelectTiles() {
            await WaitToContinue();
            SetPlayersTurn();
            _tilePlates.SetSelectionCallback(ManageSelectedTiles);

            int centerTilesExist = _tilePlates.CenterTileCount != 0 ? 1 : 0;

            var plates = _tilePlates.Plates.Where(p => !p.IsEmpty).ToList();

            for (int type = 1; type < 6; type++) {
                int tileInCenterCount = tilesInCenter((TileType)type);
                if (tileInCenterCount >= 5 && CanReasonablyFit((TileType)type, tileInCenterCount)) {
                    MoveMade.plateNr = 0;
                    _tilePlates.SelectTiles((TileType)type);
                    return;
                }
            }

            for (int count = 4; count > 0; count--) {
                for (int type = 1; type < 6; type++) {
                    var platesToSelect = platesWithCount(plates, (TileType)type, count);
                    if (platesToSelect.Count != 0 && CanReasonablyFit((TileType)type, count)) {
                        MoveMade.plateNr = (byte)(_tilePlates.Plates.IndexOf(platesToSelect[0]) + 1);
                        platesToSelect[0].SelectTiles((TileType)type);
                        return;
                    }

                    if(tilesInCenter((TileType)type) == count && CanReasonablyFit((TileType)type, count)) {
                        MoveMade.plateNr = 0;
                        _tilePlates.SelectTiles((TileType)type);
                        return;
                    }
                }
            }

            // Pick the lowest possible
            for (int count = 1; count < 5; count++) {
                for (int type = 1; type < 6; type++) {
                    var platesToSelect = platesWithCount(plates, (TileType)type, count);
                    if (platesToSelect.Count != 0) {
                        MoveMade.plateNr = (byte)(_tilePlates.Plates.IndexOf(platesToSelect[0]) + 1);
                        platesToSelect[0].SelectTiles((TileType)type);
                        return;
                    }

                    if (tilesInCenter((TileType)type) == count) {
                        MoveMade.plateNr = 0;
                        _tilePlates.SelectTiles((TileType)type);
                        return;
                    }
                }
            }

            //Select first
            _tilePlates.SelectTiles(_tilePlates.CenterTileTypes[0]);
        }

        public override async Task SelectRow() {
            await WaitToContinue();
            List<TileRow> possibleRows = [];

            for (int rowNr = 0; rowNr < tileRows.Count; rowNr++) {
                if (!tileRows[rowNr].IsFull
                    && (tileRows[rowNr].rowTileType == null || tileRows[rowNr].rowTileType == selectedTiles[0].TileType)
                    && !tileGrid.RowHasType(rowNr, selectedTiles[0].TileType))

                    possibleRows.Add(tileRows[rowNr]);
            }

            if (possibleRows.Count > 0) {

                for (int rowNr = 0; rowNr < possibleRows.Count; rowNr++) {
                    if (CanFitIntoRow(selectedTiles[0].TileType, selectedTiles.Count, possibleRows[rowNr])) {
                        TakeSelectedTiles(possibleRows[rowNr]);
                        return;
                    }
                }

                for (int rowNr = 0; rowNr < possibleRows.Count; rowNr++) {
                    if (CanReasonablyFitIntoRow(selectedTiles[0].TileType, selectedTiles.Count, 1, possibleRows[rowNr])) {
                        TakeSelectedTiles(possibleRows[rowNr]);
                        return;
                    }
                }

                TakeSelectedTiles(possibleRows.MaxBy(row => row.FreeSlotCount)!); 
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
