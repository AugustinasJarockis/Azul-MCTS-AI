using AzulBoardGame.GameState;
using AzulBoardGame.GameTilePlates;
using AzulBoardGame.Utilities;
using System.Windows.Input;
using System.Windows.Media;

namespace AzulBoardGame.Players.PlayerBase
{
    internal class Player
    {
        private PlayerUI playerUI;
        public PlayerBoardState PlayerBoardState { get; set; }
        public IPlayerAI PlayerAI;

        public int Points => PlayerBoardState.Points;
        public string Name => playerUI.Name;

        protected readonly Action NotifyAboutCompletion;

        public Player(
            PlayerBoardState playerBoardState,
            IPlayerAI playerAI,
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
            PlayerBoardState = playerBoardState;
            playerUI = new( //TODO: changing player board state requires updating link //TODO: double check correct state updates
                PlayerBoardState,
                canvasControls,
                notifyAboutCompletion,
                tilePlates,
                tileBank,
                name,
                nameColour,
                keyToFocus,
                xPos,
                yPos,
                size,
                pauseBetweenChoices
                );

            PlayerAI = playerAI;

            NotifyAboutCompletion = notifyAboutCompletion;
        }

        public void SetPlayersTurn() => playerUI.SetPlayersTurn();
        public void EndPlayersTurn() => playerUI.EndPlayersTurn();
        public bool HasFinished() => PlayerBoardState.HasFinished();
        public void CompleteRound() => playerUI.CompleteRound();
        public void CalculateAdditionalPoints() => playerUI.CalculateAdditionalPoints();

        public async Task MakeMove(GeneralGameState generalGameState) {
            SetPlayersTurn();
            var moveToMake = PlayerAI.ChooseMove(generalGameState);
            PlayerBoardState.MoveMade = moveToMake;
            await playerUI.MakeMove(moveToMake);
            EndPlayersTurn();
            NotifyAboutCompletion();
        }
    }
}
