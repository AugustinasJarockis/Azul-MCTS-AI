﻿using AzulBoardGame.Enums;
using AzulBoardGame.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AzulBoardGame.PlayerBoard
{
    internal class TileRow
    {
        private readonly Canvas _playerCanvas;
        private readonly ProcessingLine _processingLine;
        private readonly TileBank _tileBank;
        private readonly double _xPos;
        private readonly double _yPos;
        private readonly int _capacity;

        private readonly Action<TileRow> TakeSelectedTiles;

        private Border rowBorder;
        private Panel innerCanvas;

        private List<Tile> rowTiles = [];

        public TileType? rowTileType { get; private set; } = null;
        public bool IsFull => rowTiles.Count == _capacity;
        public TileRow(
            Canvas playerCanvas, 
            double xPos, 
            double yPos, 
            double height, 
            double width,
            int capacity,
            ProcessingLine processingLine,
            TileBank tileBank,
            Action<TileRow> takeSelectedTiles
            ) {
            _playerCanvas = playerCanvas;
            _processingLine = processingLine;
            _tileBank = tileBank;
            _xPos = xPos;
            _yPos = yPos;
            _capacity = capacity;
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
            if (rowTileType == null)
                rowTileType = tiles[0].TileType;

            while (rowTiles.Count < _capacity && tiles.Count > 0) {
                tiles[0].Move(_xPos - 0.0875 - rowTiles.Count * 0.092, _yPos - 0.05);
                rowTiles.Add(tiles[0]);
                tiles.RemoveAt(0);
            }
            _processingLine.AddTiles(tiles);
        }

        public Tile PrepareForTileTransfer() {
            var firstTile = rowTiles[0];
            _tileBank.DiscardTiles(rowTiles[0].TileType, rowTiles.Count - 1);

            for (int i = 1; i < rowTiles.Count; i++)
                rowTiles[i].Destroy();

            rowTiles.Clear();
            rowTileType = null;
            return firstTile;
        }

        private void ShowBorder() => rowBorder.BorderThickness = new Thickness(2.5);
        private void HideBorder() => rowBorder.BorderThickness = new Thickness(0);

        public void StopMouseInput() => innerCanvas.IsHitTestVisible = false;
        public void StartMouseInput() => innerCanvas.IsHitTestVisible = true;
    }
}
