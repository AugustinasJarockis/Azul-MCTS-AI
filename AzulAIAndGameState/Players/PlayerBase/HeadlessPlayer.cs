using AzulBoardGame.GameState;

namespace AzulBoardGame.Players.PlayerBase
{
    public class HeadlessPlayer
    {
        public PlayerBoardState PlayerBoardState { get; set; }
        public IPlayerAI PlayerAI;

        public int Points => PlayerBoardState.Points;

        public HeadlessPlayer(PlayerBoardState playerBoardState, IPlayerAI playerAI) {
            PlayerBoardState = playerBoardState;
            PlayerAI = playerAI;
        }

        public bool HasFinished() => PlayerBoardState.HasFinished();
        public void CompleteRound(ITileBank tileBank) => PlayerBoardState.CompleteRound(tileBank);
        public void CalculateAdditionalPoints() => PlayerBoardState.CalculateAdditionalPoints();

        public void MakeMove(GeneralGameState generalGameState) {
            var moveToMake = PlayerAI.ChooseMove(generalGameState);
            PlayerBoardState.MoveMade = moveToMake;
            generalGameState.MakeMove(moveToMake);
        }
    }
}
