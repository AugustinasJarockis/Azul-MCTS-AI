using AzulBoardGame.GameTilePlates;
using AzulBoardGame.Players.PlayerBase;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AzulBoardGame.Players
{
    internal class Human : Player
    {
        public Human(
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
            double size
            ) 
            : base(mainCanvas, scaleTransform, translateTransform, notifyAboutCompletion, tilePlates, tileBank, name, nameColour, keyToFocus, xPos, yPos, size) 
        {

        }

        public override async Task SelectTiles() { // TODO: Check what tile human selects
            SetPlayersTurn();
            _tilePlates.EnableUserInput();
            _tilePlates.SetSelectionCallback(ManageSelectedTiles);
        }

        public override async Task SelectRow() {
            for (int i = 0; i < tileRows.Count; i++) {
                if (!tileRows[i].IsFull
                    && (tileRows[i].rowTileType == null || tileRows[i].rowTileType == selectedTiles[0].TileType)
                    && !tileGrid.RowHasType(i, selectedTiles[0].TileType))

                    tileRows[i].StartMouseInput();
            }

            processingLine.StartMouseInput();
        }

        protected override void RemoveSelectedTiles() {
            selectedTiles.Clear();
            foreach (var row in tileRows) {
                row.StopMouseInput();
            }

            processingLine.StopMouseInput();

            EndPlayersTurn();
            NotifyAboutCompletion();
        }
    }
}
