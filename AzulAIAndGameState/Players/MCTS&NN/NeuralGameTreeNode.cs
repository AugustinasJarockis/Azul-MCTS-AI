using AzulAIAndGameState.NewFolder;
using AzulAIAndGameState.Players.MCTS_CNN;
using AzulAIAndGameState.Players.MCTS_NN.Networks;
using AzulBoardGame.Enums;
using AzulBoardGame.Extensions;
using AzulBoardGame.GameState;
using AzulBoardGame.Players.MCTS_CNN;
using TorchSharp;

namespace AzulAIAndGameState.Players.MCTS_NN
{
    public class NeuralGameTreeNode
    {
        private readonly float yFade = 0.9f;
        private readonly float lFade = 0.9f;

        private readonly NeuralGameTreeNode? _parent = null;

        private List<NeuralGameTreeNode> reachableStates = [];

        private List<(byte, TileType, byte)> possibleMoves = [];
        private float[] possibleMoveArray = new float[180];
        private float[] filteredPolicyArray = [];
        public torch.Tensor PredictedPolicy { get; private set; }
        public float NetworkValue { get; private set; } = 0;

        private PolicyNetwork _policyNetwork;
        private ValueNetwork _valueNetwork;

        public float ProbabilityToReach { get; private set; } = 0;
        public int EndsReached { get; set; } = 0;
        public double CumulativeAttemptScore { get; set; } = 0;
        public double CalculatedValue => CumulativeAttemptScore / EndsReached;

        private GeneralGameState _gameState;

        private int playerOfInterest = 0; //TODO: sutvarkyti su šituo // Nenaudojamas realiai

        public NeuralGameTreeNode(GeneralGameState gameState, PolicyNetwork policyNetwork, ValueNetwork valueNetwork, int playerOfInterest) {
            _gameState = gameState;
            _policyNetwork = policyNetwork;
            _valueNetwork = valueNetwork;
            this.playerOfInterest = playerOfInterest;

            GeneratePossibleMovesAndEval();
        }

        public NeuralGameTreeNode(NeuralGameTreeNode parent, GeneralGameState gameState, PolicyNetwork policyNetwork, ValueNetwork valueNetwork, float probabilityToReach, int playerOfInterest) {
            _parent = parent;
            _gameState = gameState;
            _policyNetwork = policyNetwork;
            _valueNetwork = valueNetwork;
            ProbabilityToReach = probabilityToReach;
            this.playerOfInterest = playerOfInterest;
        }

        public NeuralGameTreeNode GetSyncWithManager(int playerCount, GeneralGameState currentGameState) {
            if (_gameState.TilePlatesState.TotalTileCount == 0)
                return new(currentGameState.Copy(), _policyNetwork, _valueNetwork, playerOfInterest);

            if (playerCount == 0)
                return this;

            var playerToMakeAMove = (currentGameState.PlayerCount - playerCount + _gameState.CurrentPlayer) % currentGameState.PlayerCount;
            var recentMove = currentGameState.PlayerBoardStates[playerToMakeAMove].MoveMade;

            if (recentMove != null
                && possibleMoves.Contains(((byte, TileType, byte))recentMove!)
            ) {
                var indexOfMove = possibleMoves.IndexOf(((byte, TileType, byte))recentMove);
                if (indexOfMove < reachableStates.Count) {
                    return reachableStates[indexOfMove].GetSyncWithManager(playerCount - 1, currentGameState);
                }
            }
            return new(currentGameState.Copy(), _policyNetwork, _valueNetwork, playerOfInterest);
        }

        //private void Backpropagate(torch.Tensor accumulatedLoss, float advantage, float futureValue, torch.Tensor FuturePredictedPolicy) {
        //    var state = _gameState.GetListState().Flatten().Select(x => (float)x).ToArray().ToTensor([1, 121]);
        //    var policy = _policyNetwork.Call(state);
        //    var value = 2 * _valueNetwork.Call(state) - 1;
        //    int processingLinePunish = _gameState.PlayerBoardStates[_gameState.CurrentPlayer].MoveMade?.Item3 == 5 ? -100 : 0;
        //    float newAdvantage = 0 + yFade * value.item<float>() - NetworkValue + lFade * yFade * advantage;
        //    //accumulatedLoss += (FuturePredictedPolicy / policy) * newAdvantage;
        //    accumulatedLoss += -(policy / policy) * newAdvantage;
        //    //accumulatedLoss += (policy / PredictedPolicy) * newAdvantage;
        //    if (_parent != null) {
        //        _parent?.Backpropagate(accumulatedLoss, -newAdvantage, processingLinePunish, policy);
        //    }
        //    else {
        //        _policyNetwork.TrainWithLoss(accumulatedLoss - futureValue);
        //        _valueNetwork.TrainWithLoss(accumulatedLoss - futureValue);
        //    }
        //    NetworkValue = value.item<float>();
        //    CumulativeAttemptScore -= NetworkValue;
        //    EvaluatePosition(); //TODO: this is hacky
        //    CumulativeAttemptScore += NetworkValue;
        //}

        public (byte, TileType, byte) GetBestMove() => possibleMoves[reachableStates.IndexOf(reachableStates.MaxBy(s => s.EndsReached + 0.1 * s.NetworkValue))];

        public torch.Tensor GetUpdatedPolicyTensor() {
            var possibleMoveTensor = reachableStates.Select(s => s.CalculatedValue).ToArray().ToTensor([reachableStates.Count]).softmax(1);
            var correctPolicyTensor = torch.zeros([1, 6, 5, 6]);
            for (int i = 0; i < possibleMoves.Count; i++) {
                var move = possibleMoves[i];
                correctPolicyTensor[0, move.Item1, (long)move.Item2 - 1, move.Item3] = possibleMoveTensor[i];
            }

            return correctPolicyTensor;
        }

        private void EvaluatePosition() {
            var stateList = _gameState.GetListState().Flatten().Select(x => (float)x).ToList();
            float[] possibleMoveArray = GeneratePossibleMoveMatrix();
            stateList.AddRange(possibleMoveArray);

            var state = stateList.ToArray().ToTensor([1, 301]);
            var policy = _policyNetwork.Call(state).flatten();
            var value = 2 * _valueNetwork.Call(state) - 1;
            PredictedPolicy = policy;
            NetworkValue = value[0].item<float>();
        }

        private void GeneratePossibleMovesAndEval() {
            EvaluatePosition();
            CumulativeAttemptScore = NetworkValue;
            GeneratePossibleMoves();
            EndsReached = 1;
        }

        private void GeneratePossibleMoves() {
            for (int i = 0; i < 180; i++) {
                var potentialMove = MoveConverter.MoveIntToTuple(i);
                if (_gameState.IsMovePossible(potentialMove)) {
                    possibleMoves.Add(potentialMove);
                    possibleMoveArray[i] = 1.0f;
                }
            }
        }

        private float[] GeneratePossibleMoveMatrix() {
            float[] possibleMoveArray = new float[180];
            var possibleMoves = _gameState.PlayerBoardStates[_gameState.CurrentPlayer].GetPossibleMoves(_gameState.TilePlatesState);
            for (int i2 = 0; i2 < 180; i2++) {
                if (possibleMoves.Contains(MoveConverter.MoveIntToTuple(i2))) {
                    possibleMoveArray[i2] = 1.0f;
                }
            }
            return possibleMoveArray;
        }

        private void GenerateReachableStates() {
            try {

            List<torch.Tensor> evaluatableStates = [];

            while (reachableStates.Count != possibleMoves.Count) {
                var newNode = new NeuralGameTreeNode(
                    this,
                    _gameState.Copy(),
                    _policyNetwork,
                    _valueNetwork,
                    PredictedPolicy[MoveConverter.MoveTupleToInt(possibleMoves[reachableStates.Count])].item<float>(),
                    playerOfInterest
                    );
                var move = possibleMoves[reachableStates.Count];

                //var newState = newNode.MakeMove(move);
                newNode._gameState.MakeMove(move);
                newNode.GeneratePossibleMoves();

                var evaluatable = newNode._gameState.GetListState().Flatten().Select(e => (float)e).ToList();
                newNode.possibleMoveArray = newNode.GeneratePossibleMoveMatrix();
                evaluatable.AddRange(newNode.possibleMoveArray);
                evaluatableStates.Add(evaluatable.ToArray().ToTensor([1, 301]));

                //var attemptValue = newNode.NetworkValue;
                reachableStates.Add(newNode);

                //EndsReached++;
                //CumulativeAttemptScore += attemptValue;
            }

            var states = torch.stack(evaluatableStates);
            var policies = _policyNetwork.Call(states);
            var values = 2 * _valueNetwork.Call(states) - 1;
            //PredictedPolicy = policies; //TODO: this is bad
            //NetworkValue = values[0].item<float>(); //TODO: this is bad

            for (int i = 0; i < reachableStates.Count; i++) {
                reachableStates[i].PredictedPolicy = policies[i].flatten();
                reachableStates[i].NetworkValue = values[i].item<float>();
                reachableStates[i].CumulativeAttemptScore = reachableStates[i].NetworkValue;
                reachableStates[i].EndsReached = 1;
                CumulativeAttemptScore -= reachableStates[i].NetworkValue;
            }

            EndsReached += reachableStates.Count;
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        private void MakeMove((byte, TileType, byte) move) {
            _gameState.MakeMove(move);
            GeneratePossibleMovesAndEval();
        }

        public (double, int) PlayOut() {
            if (_gameState.PlayerBoardStates.Any(p => p.HasFinished()) || _gameState.TilePlatesState.TotalTileCount == 0) {
                foreach (var player in _gameState.PlayerBoardStates)
                    player.CompleteRound(_gameState.TileBankState);

                foreach (var player in _gameState.PlayerBoardStates)
                    player.CalculateAdditionalPoints();

                CumulativeAttemptScore += Math.Sign(PointDifference(_gameState.CurrentPlayer, _gameState.PlayerBoardStates));
                EndsReached++;

                //var state = _gameState.GetListState().Flatten().Select(x => (float)x).ToArray().ToTensor([1, 121]);
                //var (policy, value) = _network.Call(state);
                //_parent?.Backpropagate(accumulatedLoss: 0.0f, (float)-CumulativeAttemptScore, -value.item<float>(), policy);
                //EvaluatePosition();
                return (-CumulativeAttemptScore, 1);
            }

            if (reachableStates.Count != possibleMoves.Count) {
                GenerateReachableStates();
                return (-CumulativeAttemptScore, reachableStates.Count); //TODO: assumption here that this is the  first time after origininal visiting here
            }

            double attemptValue = 0;
            int newEndsReached = 0;
            try {

            (attemptValue, newEndsReached) = ((double, int))reachableStates.MaxBy(
                s => (-s.CumulativeAttemptScore / s.EndsReached) + 2 * (s.ProbabilityToReach * (Math.Sqrt(EndsReached) / (1 + s.EndsReached)))
                )?.PlayOut()!;
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
            
            EndsReached += newEndsReached; //TODO: this is no longer correct
            CumulativeAttemptScore += attemptValue; //TODO: this is no longer correct
            return (-attemptValue, newEndsReached);
        }
        public static double PointDifference(int playerOfInterest, List<PlayerBoardState> players) {
            var orderedPlayers = players.OrderBy(p => p.Points);
            var winningPlayer = orderedPlayers.First();
            bool isWinner = players.IndexOf(winningPlayer) == playerOfInterest;
            if (isWinner) {
                var secondPlayer = orderedPlayers.Skip(1).First();
                return winningPlayer.Points - secondPlayer.Points;
            }
            else {
                return players[playerOfInterest].Points - winningPlayer.Points;
            }
        }
    }
}
