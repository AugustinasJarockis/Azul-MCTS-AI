using AzulBoardGame.Enums;
using AzulBoardGame.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AzulBoardGame.PlayerBoard
{
    internal class ProcessingLine
    {
        private readonly Canvas _playerCanvas;
        private readonly TileBank _tileBank;

        private readonly Action DiscardSelectedTiles;

        private Canvas innerCanvas;
        private Border lineBorder;

        private List<Tile> processedTiles = [];

        public ProcessingLine(Canvas playerCanvas, TileBank tileBank, Action discardSelectedTiles) { 
            _playerCanvas = playerCanvas;
            _tileBank = tileBank;
            DiscardSelectedTiles = discardSelectedTiles;

            innerCanvas = new Canvas {
                Background = Brushes.Transparent,
                IsHitTestVisible = false
            };

            lineBorder = new Border {
                BorderBrush = Brushes.GreenYellow,
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(5),
                Child = innerCanvas
            };

            Panel.SetZIndex(lineBorder, 10);

            _playerCanvas.Loaded += (s, e) => {
                _playerCanvas.Dispatcher.BeginInvoke(() => {
                    _playerCanvas.SetRelativePosCentered(lineBorder, 0.36, 0.875, 0.245, 0.71);
                });
            };
            _playerCanvas.Children.Add(lineBorder);

            innerCanvas.MouseEnter += (o, e) => {
                ShowBorder();
            };
            innerCanvas.MouseLeave += (o, e) => {
                HideBorder();
            };

            innerCanvas.MouseDown += (o, e) => {
                if (e.LeftButton == MouseButtonState.Pressed)
                    DiscardSelectedTiles();
            };
        }
        public void AddTile(Tile tile) {
            if (processedTiles.Count < 7) {
                tile.Move(0.025 + processedTiles.Count * 0.1, 0.88);
                processedTiles.Add(tile);
            }
            else {
                _tileBank.DiscardTiles(tile.TileType);
                tile.Destroy();
            }
        }

        public void AddTiles(List<Tile> tiles) {
            foreach (var tile in tiles)
                AddTile(tile);
        }

        public int GetPointLoss() => processedTiles.Count switch {
            0 => 0,
            1 => 1,
            2 => 2,
            3 => 4,
            4 => 6,
            5 => 8,
            6 => 11,
            7 => 14,
            _ => 14
        }; 

        public int Clear() {
            int pointLoss = GetPointLoss();

            foreach (var tile in processedTiles) {
                if (tile.TileType != TileType.First)
                    _tileBank.DiscardTiles(tile.TileType);
                tile.Destroy();
            }

            processedTiles.Clear();
            return pointLoss;
        }

        private void ShowBorder() => lineBorder.BorderThickness = new Thickness(2.5);
        private void HideBorder() => lineBorder.BorderThickness = new Thickness(0);
        public void StopMouseInput() => innerCanvas.IsHitTestVisible = false;
        public void StartMouseInput() => innerCanvas.IsHitTestVisible = true;
    }
}
