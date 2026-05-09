using AzulAIAndGameState.Players.MCTS_NN.Networks;
using AzulBoardGame.Enums;
using AzulBoardGame.GameState;
using AzulBoardGame.Players.PlayerBase;
using System.Diagnostics;

namespace AzulAIAndGameState.Players.MCTS_NN
{
    public class MCTSnCustomEval : IPlayerAI
    {
        private INetwork _policyModel;
        private CustomEvalGameTreeNode gameTree;

        private bool _trainingOn = false;
        public int timeAllotedMs { get; set; } = 500;
        public MCTSnCustomEval(INetwork policyModel, int timeAllotedMs = 500, bool trainingOn = false) {
            _policyModel = policyModel;
            _trainingOn = trainingOn;
            this.timeAllotedMs = timeAllotedMs;
        }

        public (byte, TileType, byte) ChooseMove(GeneralGameState gameState) {
            if (gameTree == null) {
                gameTree = new(gameState.Copy(), _policyModel);
            }
            else {
                gameTree = gameTree.GetSyncWithManager(gameState.PlayerCount, gameState.Copy());
            }

            var timer = Stopwatch.StartNew();
            while (timer.ElapsedMilliseconds < timeAllotedMs) {
                gameTree.PlayOut();
            }
            timer.Stop();

            //Console.WriteLine("Nodes visited: " + gameTree.EndsReached);
            return gameTree.GetBestMove();
        }

        public void SaveModel(string policyPath) {
            _policyModel.Save(policyPath);
        }
    }
}
