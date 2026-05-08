using AzulBoardGame.Enums;
using AzulBoardGame.GameState;
using AzulBoardGame.Players.PlayerBase;
using System.Diagnostics;

namespace AzulAIAndGameState.Players.MiniMax
{
    public class MinimaxAI : IPlayerAI
    {
        private MiniMaxTreeNode gameTree;
        public int timeAllotedMs { get; set; } = 500;
        public MinimaxAI(int timeAllotedMs = 500) {
            this.timeAllotedMs = timeAllotedMs;
        }

        public (byte, TileType, byte) ChooseMove(GeneralGameState gameState) {
            if (gameTree == null) {
                gameTree = new(gameState.Copy());
            }
            else {
                gameTree = gameTree.GetSyncWithManager(gameState.PlayerCount, gameState.Copy());
            }

            var timer = Stopwatch.StartNew();
            while (timer.ElapsedMilliseconds < timeAllotedMs && !gameTree.BranchCompleted) {
                gameTree.DelveDeeper(timer, timeAllotedMs);
            }
            timer.Stop();

            Console.WriteLine("Nodes visited: " + gameTree.EndsReached);
            return gameTree.GetBestMove();
        }
    }
}
