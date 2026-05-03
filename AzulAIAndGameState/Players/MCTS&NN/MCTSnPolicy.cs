using AzulAIAndGameState.Players.MCTS_NN.Networks;
using AzulBoardGame.Enums;
using AzulBoardGame.GameState;
using AzulBoardGame.Players.PlayerBase;
using System.Diagnostics;
using TorchSharp;
using TorchSharp.Modules;

namespace AzulAIAndGameState.Players.MCTS_NN
{
    public class MCTSnPolicy : IPlayerAI
    {
        private PolicyNetwork _policyModel;
        private PolicyGameTreeNode gameTree;

        private bool _trainingOn = false;
        Adam? optimizer = null;
        public int timeAllotedMs { get; set; } = 500;
        public MCTSnPolicy(string policyModelPath, int timeAllotedMs = 500, bool trainingOn = false) {
            _policyModel = new(policyModelPath);
            _trainingOn = trainingOn;
            this.timeAllotedMs = timeAllotedMs;

            if (_trainingOn) {
                optimizer = torch.optim.Adam(_policyModel.parameters(), lr: 0.001);
            }
        }

        public (byte, TileType, byte) ChooseMove(GeneralGameState gameState) {
            try {

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

            Console.WriteLine("Nodes visited: " + gameTree.EndsReached);
            return gameTree.GetBestMove();
            }
            catch(Exception ex) {
                Console.WriteLine(ex.ToString());
                return (0, 0, 0);
            }
        }

        public void SaveModel(string policyPath) {
            _policyModel.save(policyPath);
        }
    }
}
