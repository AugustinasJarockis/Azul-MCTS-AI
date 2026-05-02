using AzulAIAndGameState.NewFolder;
using AzulAIAndGameState.Players.MCTS_NN.Networks;
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
            testDatasetLoader = trainDatasetLoader.SplitTestPart(3000);

            _trainingProcessData = trainingProcessData;
        }
        public void Train(PolicyNetwork model, int epochCount, int batchSize = 32) {
            Console.WriteLine("Training started");
            float minTestLoss = float.MaxValue;

            for (int epoch = 0; epoch < epochCount;  epoch++) {
                float trainingLoss = 0, testLoss = 0;
                // Training
                for (int i = 0; i < (trainDatasetLoader.DatasetSize + batchSize - 1) / batchSize; i++) {
                    (var states, var policies, var values) = trainDatasetLoader.GetBatch(batchSize);
                    var statesTensor = torch.stack(states).to(model.device);

                    var policy = model.Call(statesTensor);

                    var policyLoss = functional.cross_entropy(policy, policies);

                    model.TrainWithLoss(policyLoss);

                    trainingLoss += policyLoss.item<float>() * batchSize;

                    policy.Dispose();
                }

                // Testing
                for (int i = 0; i < (testDatasetLoader.DatasetSize + batchSize - 1) / batchSize; i++) {
                    (var states, var policies, var values) = testDatasetLoader.GetBatch(batchSize);
                    var statesTensor = torch.stack(states).to(model.device);

                    var policy = model.Call(statesTensor);

                    var policyLoss = functional.cross_entropy(policy, policies);

                    testLoss += policyLoss.item<float>() * batchSize;

                    policy.Dispose();
                }
                trainingLoss /= trainDatasetLoader.DatasetSize;
                testLoss /= testDatasetLoader.DatasetSize;

                string epochInfo = "Epoch: " + epoch + " ; Train loss: " + trainingLoss + " ; Test loss: " + testLoss + ";";
                Console.WriteLine(epochInfo);
                File.AppendAllText(_trainingProcessData, epochInfo + "\n");
                if (minTestLoss > testLoss) {
                    minTestLoss = testLoss;
                    Console.WriteLine("Saving model on epoch nr." + epoch);
                    model.save("Models/fullmodel" + epoch + ".v3.nn");
                }

                trainDatasetLoader.Shuffle();
                testDatasetLoader.Shuffle();
            }
        }

        public void TrainValue(ValueNetwork model, int epochCount, int batchSize = 32)
        {
            Console.WriteLine("Value training started");
            File.AppendAllText(_trainingProcessData, "Value training started\n");
            float minTestLoss = float.MaxValue;

            for (int epoch = 0; epoch < epochCount; epoch++)
            {
                float trainingLoss = 0, testLoss = 0;
                // Training
                for (int i = 0; i < (trainDatasetLoader.DatasetSize + batchSize - 1) / batchSize; i++)
                {
                    (var states, _, var correctValues) = trainDatasetLoader.GetBatch(batchSize);
                    var statesTensor = torch.stack(states).to(model.device);

                    var value = model.Call(statesTensor);

                    //var valueLoss = functional.binary_cross_entropy_with_logits(value.flatten(), correctValues);
                    var valueLoss = functional.smooth_l1_loss(value.flatten(), correctValues, beta: 0.5);

                    model.TrainWithLoss(valueLoss);

                    trainingLoss += valueLoss.item<float>() * batchSize;
                }

                // Testing
                for (int i = 0; i < (testDatasetLoader.DatasetSize + batchSize - 1) / batchSize; i++)
                {
                    (var states, _, var correctValues) = testDatasetLoader.GetBatch(batchSize);
                    var statesTensor = torch.stack(states).to(model.device);

                    var value = model.Call(statesTensor);

                    var valueLoss = functional.smooth_l1_loss(value.flatten(), correctValues, beta: 0.5);

                    testLoss += valueLoss.item<float>() * batchSize;
                }
                trainingLoss /= trainDatasetLoader.DatasetSize;
                testLoss /= testDatasetLoader.DatasetSize;

                string epochInfo = "Epoch: " + epoch + " ; Train loss: " + trainingLoss + " ; Test loss: " + testLoss + ";";
                Console.WriteLine(epochInfo);
                File.AppendAllText(_trainingProcessData, epochInfo + "\n");
                if (minTestLoss > testLoss)
                {
                    minTestLoss = testLoss;
                    Console.WriteLine("Saving model on epoch nr." + epoch);
                    model.save("Models/valuemodel" + epoch + ".v1.nn");
                }

                trainDatasetLoader.Shuffle();
                testDatasetLoader.Shuffle();
            }
        }

        public void TrainLegalAndNoProcessing(PolicyNetwork model, int epochCount, int batchSize = 32) {
            float minTestLoss = float.MaxValue;

            for (int epoch = 0; epoch < epochCount; epoch++) {
                float trainingLoss = 0, testLoss = 0;
                // Training
                for (int i = 0; i < (trainDatasetLoader.DatasetSize + batchSize - 1) / batchSize; i++) {
                    (var states, _, _) = trainDatasetLoader.GetBatch(batchSize);
                    var statesTensor = torch.stack(states);
                    //var (policy, _) = model.Call(statesTensor);
                    var policy = model.Call(statesTensor);

                    var correctPolicyTensor = CreatePolicyTensorWithDecreasedIllegal(policy, states, batchSize);

                    var policyLoss = functional.cross_entropy(policy.flatten(), correctPolicyTensor.flatten());

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
                    //var (policy, _) = model.Call(statesTensor);
                    var policy = model.Call(statesTensor);

                    var correctPolicyTensor = CreatePolicyTensorWithDecreasedIllegal(policy, states, batchSize);

                    var policyLoss = functional.cross_entropy(policy.flatten(), correctPolicyTensor.flatten());

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
                    model.save("Models/Legal/legalmodel" + epoch + ".v2.nn");
                }

                trainDatasetLoader.Shuffle();
                testDatasetLoader.Shuffle();
            }
        }

        private torch.Tensor CreatePolicyTensor(float[] correctMoves) {
            var correctPolicyTensor = torch.zeros([correctMoves.Length, 6, 5, 6]);
            for (int i = 0; i < correctMoves.Length; i++) {
                var move = MoveConverter.MoveIntToTuple((int)correctMoves[i]);
                correctPolicyTensor[i, move.Item1, (long)move.Item2 - 1, move.Item3] = 1;
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
            return correctPolicyTensor;
        }
    }
}
