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
        private PolicyNetwork _policyModel;
        private ValueNetwork _valueModel;
        private NeuralGameTreeNode gameTree;

        private bool _trainingOn = false;
        Adam? optimizer = null;
        public int timeAllotedMs { get; set; } = 500; 
        public MCTSnNNAI(string policyModelPath, string valueModelPath, int timeAllotedMs = 500, bool trainingOn = false) {
            _policyModel = new(policyModelPath);
            _valueModel = new(valueModelPath);
            _trainingOn = trainingOn;
            this.timeAllotedMs = timeAllotedMs;

            if (_trainingOn) {
                optimizer = torch.optim.Adam(_policyModel.parameters(), lr: 0.001);
            }
        }

        public (byte, TileType, byte) ChooseMove(GeneralGameState gameState) {
            if (gameTree == null) {
                gameTree = new(gameState.Copy(), _policyModel, _valueModel, gameState.CurrentPlayer);
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

                var policyLoss = functional.cross_entropy(predictedPolicy, correctPolicy.flatten());
                var processingLinePredictionsLoss = functional.cross_entropy(processingLinePredictions, processingLineCorrect);
                var valueLoss = functional.mse_loss(gameTree.NetworkValue, gameTree.CalculatedValue);
                torch.Tensor loss = policyLoss + processingLinePredictionsLoss * 1000 + valueLoss;

                optimizer?.zero_grad();
                loss.backward();
                optimizer?.step();

                double lossFloat = 0;
                lossFloat += (double)loss.item<double>() * 1;
                double processingLineLossFloat = 0;
                processingLineLossFloat += (double)processingLinePredictionsLoss.item<float>() * 1.0;
                Console.WriteLine("Loss: " + lossFloat + " Processing loss: " + processingLineLossFloat);
            }

            Console.WriteLine("Nodes visited: " + gameTree.EndsReached);
            return gameTree.GetBestMove();
        }

        public void SaveModel(string policyPath, string valuePath) {
            _policyModel.save(policyPath);
            _valueModel.save(valuePath);
        }
    }
}
