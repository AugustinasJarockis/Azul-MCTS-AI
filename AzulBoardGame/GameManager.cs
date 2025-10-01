using AzulBoardGame.Enums;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AzulBoardGame
{
    internal class GameManager
    {
        private readonly Canvas _mainCanvas;
        private readonly ScaleTransform _scaleTransform;
        private readonly TranslateTransform _translateTransform;
        public GameManager(Canvas mainCanvas, ScaleTransform scaleTransform, TranslateTransform translateTransform) {
            _mainCanvas = mainCanvas;
            _scaleTransform = scaleTransform;
            _translateTransform = translateTransform;

            int plateCount = 9;

            var tilePlates = new TilePlates(_mainCanvas, _scaleTransform, _translateTransform, Key.NumPad5, plateCount);

            var player1 = new Player(_mainCanvas, _scaleTransform, _translateTransform, "Test1", PlayerType.RandomAI, Key.NumPad1, 0.18, 0.82, 0.35);
            var player2 = new Player(_mainCanvas, _scaleTransform, _translateTransform, "Test2", PlayerType.RandomAI, Key.NumPad2, 0.82, 0.82, 0.35);
            var player3 = new Player(_mainCanvas, _scaleTransform, _translateTransform, "Test3", PlayerType.RandomAI, Key.NumPad3, 0.18, 0.18, 0.35);
            var player4 = new Player(_mainCanvas, _scaleTransform, _translateTransform, "Test4", PlayerType.RandomAI, Key.NumPad4, 0.82, 0.18, 0.35);

            var tileBank = new TileBank();

            var tileTypes = tileBank.RefreshTiles(plateCount);

            _mainCanvas.KeyDown += (s, e) => {
                if(e.Key == Key.R)
                    tilePlates.RefreshPlates(tileTypes);

                tilePlates.EnableUserInput();
            };
        }
    }
}
