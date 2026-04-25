using AzulAIAndGameState.NewFolder;
using AzulBoardGame.Extensions;
using AzulBoardGame.GameState;
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
            float minTestLoss = float.MaxValue;

            for (int epoch = 0; epoch < epochCount;  epoch++) {
                float trainingLoss = 0, testLoss = 0;
                // Training
                for (int i = 0; i < (trainDatasetLoader.DatasetSize + batchSize - 1) / batchSize; i++) {
                    (var states, var policies, var values) = trainDatasetLoader.GetBatch(batchSize);
                    var statesTensor = torch.stack(states);
                    var correctPolicyTensor = CreatePolicyTensor(policies);
                    var correctValuesTensor = torch.from_array(values);

                    var (policy, value) = model.Call(statesTensor);

                    var policyLoss = functional.cross_entropy(policy.flatten(), correctPolicyTensor.flatten());
                    var valueLoss = functional.mse_loss(value, correctValuesTensor.unsqueeze(1));
                    var loss = policyLoss;// + valueLoss;

                    model.TrainWithLoss(loss);

                    trainingLoss += loss.item<float>() * batchSize;
                }

                // Testing
                for (int i = 0; i < (testDatasetLoader.DatasetSize + batchSize - 1) / batchSize; i++) {
                    (var states, var policies, var values) = testDatasetLoader.GetBatch(batchSize);
                    var statesTensor = torch.stack(states);
                    var correctPolicyTensor = CreatePolicyTensor(policies);
                    var correctValuesTensor = torch.from_array(values);

                    var (policy, value) = model.Call(statesTensor);

                    var policyLoss = functional.cross_entropy(policy.flatten(), correctPolicyTensor.flatten());
                    var valueLoss = functional.mse_loss(value, correctValuesTensor.unsqueeze(1));
                    var loss = policyLoss;// + valueLoss;

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
                    model.save("Models/hmodel" + epoch + ".nn");
                }

                trainDatasetLoader.Shuffle();
                testDatasetLoader.Shuffle();
            }
        }

        public void TrainLegalAndNoProcessing(PolicyValueNetwork model, int epochCount, int batchSize = 32) {
            float minTestLoss = float.MaxValue;

            for (int epoch = 0; epoch < epochCount; epoch++) {
                float trainingLoss = 0, testLoss = 0;
                // Training
                for (int i = 0; i < (trainDatasetLoader.DatasetSize + batchSize - 1) / batchSize; i++) {
                    (var states, _, _) = trainDatasetLoader.GetBatch(batchSize);
                    var statesTensor = torch.stack(states);
                    var (policy, _) = model.Call(statesTensor);

                    var correctPolicyTensor = CreatePolicyTensorWithDecreasedIllegal(policy, states, batchSize);

                    var policyLoss = functional.cross_entropy(policy.softmax(1).flatten(), correctPolicyTensor.flatten());

                    //List<List<float>> policyValues = [];
                    //var softmaxPolicy = policy.softmax(1);
                    //for (int i2 = 0; i2 < batchSize; i2++) {
                    //    policyValues.Add([]);
                    //    for (int i3 = 0; i3 < 180; i3++) {
                    //        policyValues[i2].Add(softmaxPolicy[i2, i3].item<float>());
                    //    }
                    //}

                    model.TrainWithLoss(policyLoss);

                    trainingLoss += policyLoss.item<float>() * batchSize;
                }

                // Testing
                for (int i = 0; i < (testDatasetLoader.DatasetSize + batchSize - 1) / batchSize; i++) {
                    (var states, var policies, var values) = testDatasetLoader.GetBatch(batchSize);
                    var statesTensor = torch.stack(states);
                    var (policy, _) = model.Call(statesTensor);

                    var correctPolicyTensor = CreatePolicyTensorWithDecreasedIllegal(policy, states, batchSize);

                    var policyLoss = functional.cross_entropy(policy.softmax(1).flatten(), correctPolicyTensor.flatten());

                    testLoss += policyLoss.item<float>() * batchSize;
                }
                trainingLoss /= trainDatasetLoader.DatasetSize;
                testLoss /= testDatasetLoader.DatasetSize;

                string epochInfo = "Epoch: " + epoch + " ; Train loss: " + trainingLoss + " ; Test loss: " + testLoss + ";";
                Console.WriteLine(epochInfo);
                File.AppendAllText(_trainingProcessData, epochInfo + "\n");
                if (minTestLoss > testLoss) {
                    minTestLoss = testLoss;
                    Console.WriteLine("Saving model on epoch nr." + epoch);
                    model.save("Models/Legal/legalmodel" + epoch + ".nn");
                }

                trainDatasetLoader.Shuffle();
                testDatasetLoader.Shuffle();
            }
        }

        private torch.Tensor CreatePolicyTensor(float[] correctMoves) {
            var correctPolicyTensor = torch.zeros([correctMoves.Length, 6, 5, 6]);
            for (int i2 = 0; i2 < correctMoves.Length; i2++) {
                var move = MoveConverter.MoveIntToTuple((int)correctMoves[i2]);
                correctPolicyTensor[i2, move.Item1, (long)move.Item2 - 1, move.Item3] = 1;
            }
            return correctPolicyTensor;
        }

        private torch.Tensor CreatePolicyTensorWithDecreasedIllegal(torch.Tensor policy, torch.Tensor[] states, int batchSize) {
            var correctPolicyTensor = torch.full([batchSize, 180], float.MinValue);
            for (int i = 0; i < batchSize; i++) {
                GeneralGameState state = new(states[i].data<float>().Select(e => (int)Math.Round(e)).ToList().ExpandToGameState());
                var possibleMoves = state.PlayerBoardStates[state.CurrentPlayer].GetPossibleMoves(state.TilePlatesState);
                for (int i2 = 0; i2 < 180; i2++) {
                    var move = MoveConverter.MoveIntToTuple(i2);
                    if (move.Item3 == 5 || !possibleMoves.Contains(move)) {
                        correctPolicyTensor[i, i2] = float.MinValue;
                    }
                    else {
                        correctPolicyTensor[i, i2] = policy[i, i2].item<float>();
                    }
                }
            }
            return correctPolicyTensor.softmax(1);
        }
    }
}
