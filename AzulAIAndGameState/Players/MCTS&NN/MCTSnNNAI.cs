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

            if (false && _trainingOn) {
                var predictedPolicy = gameTree.PredictedPolicy;
                var correctPolicy = gameTree.GetUpdatedPolicyTensor();

                var predictedIndices = torch.arange(0, predictedPolicy.shape[0], 6, dtype: torch.ScalarType.Int64);

                torch.Tensor processingLinePredictions = predictedPolicy.index_select(0, predictedIndices);
                torch.Tensor processingLineCorrect = correctPolicy[0].select(2, 5).flatten();

                //var policyLoss = functional.cross_entropy(predictedPolicy, correctPolicy.flatten());
                var processingLinePredictionsLoss = functional.cross_entropy(processingLinePredictions, processingLineCorrect);
                //var valueLoss = functional.mse_loss(gameTree.NetworkValue, gameTree.CalculatedValue);
                //torch.Tensor loss = policyLoss + processingLinePredictionsLoss * 1000 + valueLoss;

                //optimizer?.zero_grad();
                //loss.backward();
                //optimizer?.step();

                double lossFloat = 0;
                //lossFloat += (double)loss.item<double>() * 1;
                double processingLineLossFloat = 0;
                processingLineLossFloat += (double)processingLinePredictionsLoss.item<float>() * 1.0;
                Console.WriteLine("Loss: " + lossFloat + " Processing loss: " + processingLineLossFloat);
            }

            //var targetPolicy = gameTree.GetMCTSUpdatedPolicy();
            //var predictedPolicyLog = functional.log_softmax(gameTree.PredictedPolicy, 0);
            //var policyLoss = -(targetPolicy * predictedPolicyLog).sum().mean();
            //_policyModel.TrainWithLoss(policyLoss);

            //var valueLoss = functional.smooth_l1_loss(gameTree.ValuePrediction.squeeze(), (float)gameTree.CalculatedValue, beta: 0.5);
            //_valueModel.TrainWithLoss(valueLoss);

            //double lossValue = (double)valueLoss.item<float>();
            //Console.WriteLine("Nodes visited: " + gameTree.EndsReached + " | Loss: " + lossValue.ToString("F15"));
            //File.AppendAllText("LossValueAttemptLong.txt", lossValue + "\n");

            return gameTree.GetBestMove();
        }

        public void SaveModel(string policyPath, string valuePath) {
            _policyModel.Save(policyPath);
            _valueModel.Save(valuePath);
        }
    }
}
