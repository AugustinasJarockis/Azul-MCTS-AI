using AzulBoardGame.Enums;
using AzulBoardGame.Extensions;
using AzulBoardGame.GameState;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AzulBoardGame.PlayerBoard.PlayerTileRow
{
    internal class TileRow : ITileRow
    {
        public TileRowState State { get; set; }

        private readonly Canvas _playerCanvas;
        private readonly ProcessingLine _processingLine;
        private readonly ITileBank _tileBank;
        private readonly double _xPos;
        private readonly double _yPos;

        private readonly Action<TileRow> TakeSelectedTiles;

        private Border rowBorder;
        private Panel innerCanvas;

        private List<Tile> rowTiles = [];
        public TileType? RowTileType => State.RowTileType;
        public bool IsFull => State.IsFull;
        public bool IsEmpty => State.IsEmpty;
        public int TileCount => State.TileCount;
        public int FreeSlotCount => State.FreeSlotCount;
        public TileRow(
            TileRowState state,
            Canvas playerCanvas, 
            double xPos, 
            double yPos, 
            double height, 
            double width,
            int capacity,
            ProcessingLine processingLine,
            ITileBank tileBank,
            Action<TileRow> takeSelectedTiles
            ) {
            State = state;
            _playerCanvas = playerCanvas;
            _processingLine = processingLine;
            _tileBank = tileBank;
            _xPos = xPos;
            _yPos = yPos;
            TakeSelectedTiles = takeSelectedTiles;

            innerCanvas = new Canvas {
                Background = Brushes.Transparent,
                IsHitTestVisible = false
            };

            rowBorder = new Border {
                BorderBrush = Brushes.GreenYellow,
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(5),
                Child = innerCanvas
            };

            Panel.SetZIndex(rowBorder, 10);

            _playerCanvas.Loaded += (s, e) => {
                _playerCanvas.Dispatcher.BeginInvoke(() => {
                    _playerCanvas.SetRelativePosCentered(rowBorder, xPos - width / 2, yPos, height, width);
                });
            };
            _playerCanvas.Children.Add(rowBorder);

            innerCanvas.MouseEnter += (o, e) => {
                ShowBorder();
            };
            innerCanvas.MouseLeave += (o, e) => {
                HideBorder();
            };

            innerCanvas.MouseDown += (o, e) => {
                if (e.LeftButton == MouseButtonState.Pressed)
                    TakeSelectedTiles(this);
            };
        }

        public void AddTiles(List<Tile> tiles) {
            State.AddTiles([.. tiles.Select(t => t.TileType)]);
            while (rowTiles.Count < State.Capacity && tiles.Count > 0) {
                tiles[0].Move(_xPos - 0.0875 - rowTiles.Count * 0.092, _yPos - 0.05);
                rowTiles.Add(tiles[0]);
                tiles.RemoveAt(0);
            }
            _processingLine.AddTiles(tiles);
        }

        public Tile PrepareForTileTransfer() {
            State.PrepareForTileTransfer(); //TODO: Pazet ar viskas gerai, kad nediscardinam realiai
            var firstTile = rowTiles[0];
            _tileBank.DiscardTiles(rowTiles[0].TileType, rowTiles.Count - 1); //Šitas būtinas, nes state to nedaro

            for (int i = 1; i < rowTiles.Count; i++)
                rowTiles[i].Destroy();

            rowTiles.Clear();
            return firstTile;
        }

        private void ShowBorder() => rowBorder.BorderThickness = new Thickness(2.5);
        private void HideBorder() => rowBorder.BorderThickness = new Thickness(0);

        public void StopMouseInput() => innerCanvas.IsHitTestVisible = false;
        public void StartMouseInput() => innerCanvas.IsHitTestVisible = true;
    }
}
