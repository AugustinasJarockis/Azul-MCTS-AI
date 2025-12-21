using AzulBoardGame.Enums;
using AzulBoardGame.Players.PlayerBase;

namespace AzulBoardGame.Players.MCTS.StateEvaluators
{
    internal class GenericStateEvaluator : IStateEvaluator
    {
        private readonly Func<List<(byte, TileType, byte)>, List<GameState>, (byte, TileType, byte)> BestMoveFunc;
        private readonly Func<int, List<PlayerState<HeuristicStateDelver>>, double> EndPositionScoreFunc;
        private readonly Func<List<GameState>, double> PositionScoreFunc;
        public GenericStateEvaluator(
            Func<List<(byte, TileType, byte)>, List<GameState>, (byte, TileType, byte)> BestMoveFunc,
            Func<int, List<PlayerState<HeuristicStateDelver>>, double> EndPositionScoreFunc,
            Func<List<GameState>, double> PositionScoreFunc
            ) { 
            this.BestMoveFunc = BestMoveFunc;
            this.EndPositionScoreFunc = EndPositionScoreFunc;
            this.PositionScoreFunc = PositionScoreFunc;
        }
        public (byte, TileType, byte) GetBestMove(List<(byte, TileType, byte)> possibleMoves, List<GameState> reachableStates) 
            => BestMoveFunc(possibleMoves, reachableStates);
        public double GetEndPositionScore(int playerOfInterest, List<PlayerState<HeuristicStateDelver>> players) 
            => EndPositionScoreFunc(playerOfInterest, players);

        public double GetPositionScore(List<GameState> reachableStates) 
            => PositionScoreFunc(reachableStates);

        // Best move selection functions
        public static (byte, TileType, byte) MaxScore(List<(byte, TileType, byte)> possibleMoves, List<GameState> reachableStates)
            => possibleMoves[reachableStates.IndexOf(reachableStates.MaxBy(s => s.Score))];

        public static (byte, TileType, byte) MaxVisit(List<(byte, TileType, byte)> possibleMoves, List<GameState> reachableStates)
            => possibleMoves[reachableStates.IndexOf(reachableStates.MaxBy(s => s.EndsReached))];

        // End position evaluation functions
        public static double PointDifference(int playerOfInterest, List<PlayerState<HeuristicStateDelver>> players) {
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

        public static double PointTotal(int playerOfInterest, List<PlayerState<HeuristicStateDelver>> players) {
            return players[playerOfInterest].Points;
        }

        // Intermediate position evaluation functions
        public static double AveragePoints(List<GameState> reachableStates) {
            double scoreSum = 0;
            foreach (var state in reachableStates) {
                scoreSum += state.Score;
            }
            return scoreSum / reachableStates.Count;
        }

        public static double MaxPoints(List<GameState> reachableStates) {
            return reachableStates.Max(s => s.Score);
        }
    }
}
