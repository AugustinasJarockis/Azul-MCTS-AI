using AzulBoardGame.Enums;
using AzulBoardGame.GameState;
using AzulBoardGame.Players.MCTS.StateEvaluators;
using AzulBoardGame.Players.PlayerBase;
using System.Diagnostics;

namespace AzulBoardGame.Players.MCTS
{
    public class MCTSAI : IPlayerAI
    {
        public int timeAllotedMs = 500;
        private GameTreeNode gameTree;
        private readonly IStateEvaluator _stateEvaluator;
        public MCTSAI(IStateEvaluator evaluator) {
            _stateEvaluator = evaluator;
        }

        public (byte, TileType, byte) SelectTiles(GeneralGameState gameState) {
            // Logic
            if (gameTree == null) {
                gameTree = new(gameState.Copy(), _stateEvaluator, gameState.CurrentPlayer);
            }
            else {
                gameTree = gameTree.GetSyncWithManager(gameState.PlayerCount, gameState.Copy());
            }

            var timer = Stopwatch.StartNew();
            while (timer.ElapsedMilliseconds < timeAllotedMs) {
                gameTree.PlayOut(true);
            }
            timer.Stop();

            Console.WriteLine("Nodes visited: " + gameTree.EndsReached);
            return gameTree.GetBestMove();
        }

        public (byte, TileType, byte) ChooseMove(GeneralGameState gameState) {
            return SelectTiles(gameState);
        }
    }
}
