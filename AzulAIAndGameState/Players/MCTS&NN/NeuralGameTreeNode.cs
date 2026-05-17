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
        private List<NeuralGameTreeNode> reachableStates = [];

        private List<(byte, TileType, byte)> possibleMoves = [];
        private float[] possibleMoveArray = new float[180];
        private float[] filteredPolicyArray = [];
        public torch.Tensor PredictedPolicy { get; private set; }
        public torch.Tensor ValuePrediction { get; private set; }
        public float NetworkValue { get; private set; } = 0;

        private INetwork _policyNetwork;
        private INetwork _valueNetwork;

        public float ProbabilityToReach { get; private set; } = 0;
        public int EndsReached { get; set; } = 0;
        public double CumulativeAttemptScore { get; set; } = 0;
        public double CalculatedValue => CumulativeAttemptScore / EndsReached;

        private GeneralGameState _gameState;

        public NeuralGameTreeNode(GeneralGameState gameState, INetwork policyNetwork, INetwork valueNetwork) {
            _gameState = gameState;
            _policyNetwork = policyNetwork;
            _valueNetwork = valueNetwork;

            GeneratePossibleMovesAndEval();
        }

        public NeuralGameTreeNode(GeneralGameState gameState, INetwork policyNetwork, INetwork valueNetwork, float probabilityToReach) {
            _gameState = gameState;
            _policyNetwork = policyNetwork;
            _valueNetwork = valueNetwork;
            ProbabilityToReach = probabilityToReach;
        }

        public NeuralGameTreeNode GetSyncWithManager(int playerCount, GeneralGameState currentGameState) {
            if (_gameState.TilePlatesState.TotalTileCount == 0)
                return new(currentGameState.Copy(), _policyNetwork, _valueNetwork);

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
            return new(currentGameState.Copy(), _policyNetwork, _valueNetwork);
        }

        //public (byte, TileType, byte) GetBestMove() => possibleMoves[reachableStates.IndexOf(reachableStates.MaxBy(s => s.CalculatedValue))];
        public (byte, TileType, byte) GetBestMove() => possibleMoves[reachableStates.IndexOf(reachableStates.MaxBy(s => s.EndsReached - 0.1 * s.NetworkValue))];

        private void EvaluatePosition() {
            var stateList = _gameState.GetListState().Flatten().Select(x => (float)x).ToList();
            float[] possibleMoveArray = GeneratePossibleMoveMatrix();
            stateList.AddRange(possibleMoveArray);

            var state = stateList.ToArray().ToTensor([1, 301]);
            var policy = _policyNetwork.Call(state).flatten();
            var value = _valueNetwork.Call(state);
            PredictedPolicy = policy;
            ValuePrediction = value[0];
            NetworkValue = value[0].item<float>();
        }

        private void GeneratePossibleMovesAndEval() {
            EvaluatePosition();
            CumulativeAttemptScore = NetworkValue;
            GeneratePossibleMoves();
            GenerateFilteredPolicyArray();
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

        public float[] GetMCTSUpdatedPolicy() {
            var policyTarget = new float[180];
            for (int i = 0; i < reachableStates.Count; i++) {
                int moveMade = MoveConverter.MoveTupleToInt(possibleMoves[i]);
                policyTarget[moveMade] = (float)reachableStates[i].EndsReached / (float)EndsReached;
            }

            return policyTarget;
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

        private void GenerateFilteredPolicyArray() {
            var filteredPolicyTensor = torch.empty([possibleMoves.Count]);
            for (int i = 0; i < possibleMoves.Count; i++) {
                filteredPolicyTensor[i] = PredictedPolicy[MoveConverter.MoveTupleToInt(possibleMoves[i])];
            }
            filteredPolicyArray = filteredPolicyTensor.softmax(0).data<float>().ToArray();
        }

        private void GenerateReachableStates() {
            List<torch.Tensor> evaluatableStates = [];

            while (reachableStates.Count != possibleMoves.Count) {
                var newNode = new NeuralGameTreeNode(
                    _gameState.Copy(),
                    _policyNetwork,
                    _valueNetwork,
                    filteredPolicyArray[reachableStates.Count]
                    );
                var move = possibleMoves[reachableStates.Count];

                newNode._gameState.MakeMove(move);
                newNode.GeneratePossibleMoves(); // TODO: check if needed

                var evaluatable = newNode._gameState.GetListState().Flatten().Select(e => (float)e).ToList();
                newNode.possibleMoveArray = newNode.GeneratePossibleMoveMatrix();
                evaluatable.AddRange(newNode.possibleMoveArray);
                evaluatableStates.Add(evaluatable.ToArray().ToTensor([301]));
                reachableStates.Add(newNode);
            }

            var states = torch.stack(evaluatableStates);
            var policies = _policyNetwork.Call(states);
            var valuesPredition = _valueNetwork.Call(states);
            var values = valuesPredition.data<float>().ToArray();

            //List<float> policyData = [];
            //for (int i = 0; i < reachableStates.Count; i++) {
            //    List<float> array = policies[i].data<float>().ToList();
            //    policyData.Add(array.Sum());
            //}
            //float[] valuesData = values.flatten().data<float>().ToArray();

            for (int i = 0; i < reachableStates.Count; i++) {
                reachableStates[i].PredictedPolicy = policies[i].flatten();
                reachableStates[i].GenerateFilteredPolicyArray();
                reachableStates[i].ValuePrediction = valuesPredition[i];
                reachableStates[i].NetworkValue = values[i];
                reachableStates[i].CumulativeAttemptScore = reachableStates[i].NetworkValue;
                reachableStates[i].EndsReached = 1;
                CumulativeAttemptScore -= reachableStates[i].NetworkValue;
            }

            EndsReached += reachableStates.Count;
        }

        public (double, int) PlayOut() {
            if (_gameState.PlayerBoardStates.Any(p => p.HasFinished()) || _gameState.TilePlatesState.TotalTileCount == 0) {
                foreach (var player in _gameState.PlayerBoardStates)
                    player.CompleteRound(_gameState.TileBankState);

                foreach (var player in _gameState.PlayerBoardStates)
                    player.CalculateAdditionalPoints();

                int pointSum = _gameState.PlayerBoardStates.Sum(s => s.Points);
                double score = pointSum != 0 ? PointDifference(_gameState.CurrentPlayer, _gameState.PlayerBoardStates) / pointSum : 0;
                CumulativeAttemptScore += score;
                EndsReached++;

                return (-score, 1);
            }

            if (reachableStates.Count != possibleMoves.Count) {
                GenerateReachableStates();
                return (-CumulativeAttemptScore, reachableStates.Count);
            }

            var (attemptValue, newEndsReached) = ((double, int))reachableStates.MaxBy(
                s => (-s.CumulativeAttemptScore / s.EndsReached) + 2 * (s.ProbabilityToReach * (Math.Sqrt(EndsReached) / (1 + s.EndsReached)))
                )?.PlayOut()!;
            
            EndsReached += newEndsReached;
            CumulativeAttemptScore += attemptValue;
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
