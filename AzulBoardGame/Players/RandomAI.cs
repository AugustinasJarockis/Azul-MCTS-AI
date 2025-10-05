using AzulBoardGame.PlayerBoard;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AzulBoardGame.Players
{
    internal class RandomAI : Player
    {
        private Random rnd = new(DateTime.Now.Microsecond * DateTime.Now.Millisecond);
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
            double size
            )
            : base(mainCanvas, scaleTransform, translateTransform, notifyAboutCompletion, tilePlates, tileBank, name, nameColour, keyToFocus, xPos, yPos, size) {

        }

        public override void SelectTiles() {
            SetPlayersTurn();
            _tilePlates.SetSelectionCallback(ManageSelectedTiles);

            var plates = _tilePlates.Plates.Where(p => !p.IsEmpty).ToList();
            int selection = rnd.Next(plates.Count + 1);

            if (selection == 0) {
                var centerTileTypes = _tilePlates.CenterTileTypes;
                int tileToSelect = rnd.Next(_tilePlates.CenterTileTypes.Count);
                var selectedType = centerTileTypes[tileToSelect];
                _tilePlates.SelectTiles(selectedType);
            }
            else {
                int tileToSelect = rnd.Next(4);
                var selectedType = plates[selection - 1].TileTypes[tileToSelect];
                plates[selection - 1].SelectTiles(selectedType);
            }
        }

        public override void SelectRow() {
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
    }
}
