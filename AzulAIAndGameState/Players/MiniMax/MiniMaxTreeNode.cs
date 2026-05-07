using AzulBoardGame.Enums;
using AzulBoardGame.GameState;
using System.Diagnostics;

namespace AzulAIAndGameState.Players.MiniMax
{
    public class MiniMaxTreeNode
    {
        private List<MiniMaxTreeNode> reachableStates = [];

        private List<(byte, TileType, byte)> possibleMoves = [];
        public double EstimatedValue { get; private set; } = 0;
        public int EndsReached { get; set; } = 0;
        public bool BranchCompleted = false;

        private GeneralGameState _gameState;

        public MiniMaxTreeNode(GeneralGameState gameState, bool eval = true) {
            _gameState = gameState;

            if (eval)
                GeneratePossibleMovesAndEval();
        }

        public MiniMaxTreeNode GetSyncWithManager(int playerCount, GeneralGameState currentGameState) {
            if (_gameState.TilePlatesState.TotalTileCount == 0)
                return new(currentGameState.Copy());

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
            return new(currentGameState.Copy());
        }

        public (byte, TileType, byte) GetBestMove() => possibleMoves[reachableStates.IndexOf(reachableStates.MaxBy(s => -s.EstimatedValue))];
        //public (byte, TileType, byte) GetBestMove() => possibleMoves[reachableStates.IndexOf(reachableStates.MaxBy(s => s.EndsReached - 0.1 * s.NetworkValue))];

        private void GeneratePossibleMovesAndEval() {
            EstimatedValue = _gameState.EstimatePositionValue();
            possibleMoves = _gameState.PlayerBoardStates[_gameState.CurrentPlayer].GetPossibleMoves(_gameState.TilePlatesState);
            EndsReached = 1;
        }
        private void GenerateReachableStates() {
            try {
                while (reachableStates.Count != possibleMoves.Count) {
                    var newNode = new MiniMaxTreeNode(_gameState.Copy(), false);
                    var move = possibleMoves[reachableStates.Count];

                    newNode._gameState.MakeMove(move);
                    newNode.GeneratePossibleMovesAndEval();
                    reachableStates.Add(newNode);
                }
                EndsReached += reachableStates.Count;
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        public int DelveDeeper(Stopwatch timer, int timeAllotedMs) {
            if (_gameState.PlayerBoardStates.Any(p => p.HasFinished()) || _gameState.TilePlatesState.TotalTileCount == 0) {
                foreach (var player in _gameState.PlayerBoardStates)
                    player.CompleteRound(_gameState.TileBankState);

                foreach (var player in _gameState.PlayerBoardStates)
                    player.CalculateAdditionalPoints();

                //EstimatedValue = 1000 * PointDifference(_gameState.CurrentPlayer, _gameState.PlayerBoardStates);
                EstimatedValue = _gameState.EstimatePositionValue();
                EndsReached++;
                BranchCompleted = true;
                return 1;
            }

            if (reachableStates.Count != possibleMoves.Count) {
                GenerateReachableStates();
                return reachableStates.Count;
            }

            int newEndsReached = 0;
            double minScore = reachableStates.Min(s => -s.EstimatedValue);
            double maxScore = reachableStates.Max(s => -s.EstimatedValue);
            try {
                var stateToAttempt = reachableStates
                        .Where(
                        s => ((-s.EstimatedValue - minScore) / (maxScore - minScore)) + 2 * (1 - (s.EndsReached / EndsReached)) > 0.9
                        && !s.BranchCompleted
                        ).ToList();

                if (stateToAttempt.Count() != 0) {  
                    for (int i = 0; i < stateToAttempt.Count() && timer.ElapsedMilliseconds < timeAllotedMs; i++) {
                        newEndsReached += stateToAttempt[i].DelveDeeper(timer, timeAllotedMs);
                    }
                }
                else {
                    var toVisitAnyway = reachableStates.Where(s => !s.BranchCompleted).ToList();
                    for (int i = 0; i < toVisitAnyway.Count() && timer.ElapsedMilliseconds < timeAllotedMs; i++) {
                        newEndsReached += toVisitAnyway[i].DelveDeeper(timer, timeAllotedMs);
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            EndsReached += newEndsReached;
            EstimatedValue = reachableStates.Max(s => -s.EstimatedValue);
            BranchCompleted = reachableStates.All(s => s.BranchCompleted);
            return newEndsReached;
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
