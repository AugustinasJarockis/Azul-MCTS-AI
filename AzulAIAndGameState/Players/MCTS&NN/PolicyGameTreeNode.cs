using AzulAIAndGameState.NewFolder;
using AzulAIAndGameState.Players.MCTS_NN.Networks;
using AzulBoardGame.Enums;
using AzulBoardGame.Extensions;
using AzulBoardGame.GameState;
using TorchSharp;

namespace AzulAIAndGameState.Players.MCTS_NN
{
    public class PolicyGameTreeNode
    {
        private readonly PolicyGameTreeNode? _parent = null;

        private List<PolicyGameTreeNode> reachableStates = [];

        private List<(byte, TileType, byte)> possibleMoves = [];
        private float[] possibleMoveArray = new float[180];
        private float[] filteredPolicyArray = [];
        public torch.Tensor PredictedPolicy { get; private set; }
        private float[] filteredPredictedPolicy;
        private INetwork _policyNetwork;

        public float ProbabilityToReach { get; private set; } = 0;
        public int EndsReached { get; set; } = 0;
        public double CumulativeAttemptScore { get; set; } = 0;
        public double CalculatedValue => CumulativeAttemptScore / EndsReached;

        private GeneralGameState _gameState;

        public PolicyGameTreeNode(GeneralGameState gameState, INetwork policyNetwork) {
            _gameState = gameState;
            _policyNetwork = policyNetwork;

            GeneratePossibleMovesAndEval();
        }

        public PolicyGameTreeNode(PolicyGameTreeNode parent, GeneralGameState gameState, INetwork policyNetwork, float probabilityToReach) {
            _parent = parent;
            _gameState = gameState;
            _policyNetwork = policyNetwork;
            ProbabilityToReach = probabilityToReach;
        }

        public PolicyGameTreeNode GetSyncWithManager(int playerCount, GeneralGameState currentGameState) {
            if (_gameState.TilePlatesState.TotalTileCount == 0)
                return new(currentGameState.Copy(), _policyNetwork);

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
            return new(currentGameState.Copy(), _policyNetwork);
        }

        public (byte, TileType, byte) GetBestMove() => possibleMoves[reachableStates.IndexOf(reachableStates.MaxBy(s => s.EndsReached))];

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
            PredictedPolicy = policy;
        }

        private void GeneratePossibleMovesAndEval() {
            EvaluatePosition();
            CumulativeAttemptScore = 0;
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
                var newNode = new PolicyGameTreeNode(
                    this,
                    _gameState.Copy(),
                    _policyNetwork,
                    filteredPolicyArray[reachableStates.Count]
                    );
                var move = possibleMoves[reachableStates.Count];

                newNode._gameState.MakeMove(move);
                newNode.GeneratePossibleMoves();

                var evaluatable = newNode._gameState.GetListState().Flatten().Select(e => (float)e).ToList();
                newNode.possibleMoveArray = newNode.GeneratePossibleMoveMatrix();
                evaluatable.AddRange(newNode.possibleMoveArray);
                evaluatableStates.Add(evaluatable.ToArray().ToTensor([301]));

                reachableStates.Add(newNode);
            }

            var states = torch.stack(evaluatableStates);
            var policies = _policyNetwork.Call(states);
            List<float> policyData = [];
            for (int i = 0; i < reachableStates.Count; i++) {
                List<float> array = policies[i].data<float>().ToList();
                policyData.Add(array.Sum());
            }

            for (int i = 0; i < reachableStates.Count; i++) {
                reachableStates[i].PredictedPolicy = policies[i].flatten();
                reachableStates[i].GenerateFilteredPolicyArray();
                reachableStates[i].CumulativeAttemptScore = 0;
                reachableStates[i].EndsReached = 1;
            }

            EndsReached += reachableStates.Count;
        }

        public double PlayOut() {
            if (_gameState.PlayerBoardStates.Any(p => p.HasFinished()) || _gameState.TilePlatesState.TotalTileCount == 0) {
                foreach (var player in _gameState.PlayerBoardStates)
                    player.CompleteRound(_gameState.TileBankState);

                foreach (var player in _gameState.PlayerBoardStates)
                    player.CalculateAdditionalPoints();

                //double score = Math.Sign(PointDifference(_gameState.CurrentPlayer, _gameState.PlayerBoardStates));
                //double score = PointDifference(_gameState.CurrentPlayer, _gameState.PlayerBoardStates);
                int pointSum = _gameState.PlayerBoardStates.Sum(s => s.Points);
                double score = pointSum != 0 ? PointDifference(_gameState.CurrentPlayer, _gameState.PlayerBoardStates) / pointSum : 0;
                CumulativeAttemptScore += score;
                EndsReached++;
                return -score;
            }

            if (reachableStates.Count != possibleMoves.Count) {
                GenerateReachableStates();
            }

            double attemptValue = 0;
            attemptValue = (double)reachableStates.MaxBy(
                s => (-s.CumulativeAttemptScore / s.EndsReached) + 2 * (s.ProbabilityToReach * (Math.Sqrt(EndsReached) / (1 + s.EndsReached)))
                )?.PlayOut()!;

            EndsReached++;
            CumulativeAttemptScore += attemptValue;
            return -attemptValue;
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
