using AzulAIAndGameState.NewFolder;
using AzulBoardGame.Players.MCTS_CNN;
using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch.nn;

namespace AzulAIAndGameState.Players.MCTS_CNN
{
    public class NetworkTrainer
    {
        private string _trainingProcessData;

        DatasetLoader trainDatasetLoader;
        DatasetLoader testDatasetLoader;
        public NetworkTrainer(string filename, string trainingProcessData) {
            trainDatasetLoader = new(filename);
            testDatasetLoader = trainDatasetLoader.SplitTestPart(1000);

            _trainingProcessData = trainingProcessData;
        }
        public void Train(PolicyValueNetwork model, int epochCount, int batchSize = 32) {
            Adam optimizer = torch.optim.Adam(model.parameters(), lr: 0.001);
            float minTestLoss = float.MaxValue;

            for (int epoch = 0; epoch < epochCount;  epoch++) {
                float trainingLoss = 0, testLoss = 0;
                // Training
                for (int i = 0; i < (trainDatasetLoader.DatasetSize + batchSize - 1) / batchSize; i++) {
                    (var states, var policies, var values) = trainDatasetLoader.GetBatch(batchSize);
                    var statesTensor = torch.stack(states);

                    var correctPolicyTensor = torch.zeros([batchSize, 6, 5, 6]);
                    for(int i2 = 0; i2 < policies.Length; i2++) {
                        var move = MoveConverter.MoveIntToTuple((int)policies[i2]);
                        correctPolicyTensor[i2, move.Item1, (long)move.Item2 - 1, move.Item3] = 1;
                    }

                    var correctValuesTensor = torch.from_array(values);

                    var (policy, value) = model.Call(statesTensor);

                    var policyLoss = functional.cross_entropy(policy.flatten(), correctPolicyTensor.flatten());
                    var valueLoss = functional.mse_loss(value, correctValuesTensor.unsqueeze(1));
                    var loss = policyLoss * 0.01 + valueLoss;

                    optimizer.zero_grad();
                    loss.backward();
                    optimizer.step();

                    trainingLoss += loss.item<float>() * batchSize;
                }

                // Testing
                for (int i = 0; i < (testDatasetLoader.DatasetSize + batchSize - 1) / batchSize; i++) {
                    (var states, var policies, var values) = testDatasetLoader.GetBatch(batchSize);
                    var statesTensor = torch.stack(states);

                    var correctPolicyTensor = torch.zeros([batchSize, 6, 5, 6]);
                    for (int i2 = 0; i2 < policies.Length; i2++) {
                        var move = MoveConverter.MoveIntToTuple((int)policies[i2]);
                        correctPolicyTensor[i2, move.Item1, (long)move.Item2 - 1, move.Item3] = 1;
                    }

                    var correctValuesTensor = torch.from_array(values);

                    var (policy, value) = model.Call(statesTensor);

                    var policyLoss = functional.cross_entropy(policy.flatten(), correctPolicyTensor.flatten());
                    var valueLoss = functional.mse_loss(value, correctValuesTensor.unsqueeze(1));
                    var loss = policyLoss * 0.01 + valueLoss;

                    testLoss += loss.item<float>() * batchSize;
                }
                trainingLoss /= trainDatasetLoader.DatasetSize;
                testLoss /= testDatasetLoader.DatasetSize;

                string epochInfo = "Epoch: " + epoch + " ; Train loss: " + trainingLoss + " ; Test loss: " + testLoss + ";";
                Console.WriteLine(epochInfo);
                File.AppendAllText(_trainingProcessData, epochInfo + "\n");
                if (minTestLoss > testLoss) {
                    minTestLoss = testLoss;
                    Console.WriteLine("Saving model on epoch nr." + epoch);
                    model.save("Models/model92-" + epoch + ".nn");
                }

                trainDatasetLoader.Shuffle();
                testDatasetLoader.Shuffle();
            }
        }
    }
}
