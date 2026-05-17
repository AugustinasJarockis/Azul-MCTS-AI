using AzulAIAndGameState.Players.MCTS_NN.Networks;
using AzulBoardGame.Enums;
using AzulBoardGame.GameState;
using AzulBoardGame.Players.MCTS_CNN;
using AzulBoardGame.Players.PlayerBase;
using System.Diagnostics;
using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch.nn;

namespace AzulAIAndGameState.Players.MCTS_NN
{
    public class MCTSnNNAI : IPlayerAI
    {
        private INetwork _policyModel;
        private INetwork _valueModel;
        private NeuralGameTreeNode gameTree;

        private bool _trainingOn = false;
        public int timeAllotedMs { get; set; } = 500; 
        public MCTSnNNAI(INetwork policyModel, INetwork valueModel, int timeAllotedMs = 500, bool trainingOn = false) {
            _policyModel = policyModel;
            _valueModel = valueModel;
            _trainingOn = trainingOn;
            this.timeAllotedMs = timeAllotedMs;
        }

        public (byte, TileType, byte) ChooseMove(GeneralGameState gameState) {
            if (gameTree == null) {
                gameTree = new(gameState.Copy(), _policyModel, _valueModel);
            }
            else {
                gameTree = gameTree.GetSyncWithManager(gameState.PlayerCount, gameState.Copy());
            }

            var timer = Stopwatch.StartNew();
            while (timer.ElapsedMilliseconds < timeAllotedMs) {
                gameTree.PlayOut();
            }
            timer.Stop();

            if (_trainingOn) {
                var targetPolicy = gameTree.GetMCTSUpdatedPolicy();
                var predictedPolicyLog = functional.log_softmax(gameTree.PredictedPolicy, 0) - gameTree.CalculatedValue;
                var policyLoss = -(targetPolicy * predictedPolicyLog).sum().mean();
                _policyModel.TrainWithLoss(policyLoss);

                var valueLoss = functional.mse_loss(gameTree.ValuePrediction.squeeze(), (float)gameTree.CalculatedValue);
                _valueModel.TrainWithLoss(valueLoss);

                double lossValue = (double)valueLoss.item<float>();
                Console.WriteLine("Nodes visited: " + gameTree.EndsReached + " | Loss: " + lossValue.ToString("F15") +  " | Network value: " + gameTree.NetworkValue + " | Calculated value: " + ((float)gameTree.CalculatedValue).ToString("F15"));
                File.AppendAllText("LossValueAttemptLongNr3.txt", lossValue + "\n");
            }

            return gameTree.GetBestMove();
        }

        public void SaveModel(string policyPath, string valuePath) {
            _policyModel.Save(policyPath);
            _valueModel.Save(valuePath);
        }
    }
}
