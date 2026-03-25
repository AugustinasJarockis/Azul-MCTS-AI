using AzulBoardGame.Enums;
using AzulBoardGame.GameState;

namespace AzulBoardGame.Players.MCTS.StateEvaluators
{
    internal interface IStateEvaluator
    {
        public (byte, TileType, byte) GetBestMove(List<(byte, TileType, byte)> possibleMoves, List<GameTreeNode> reachableStates);
        public double GetPositionScore(List<GameTreeNode> reachableStates);
        public double GetEndPositionScore(int playerOfInterest, List<PlayerBoardState> players);
    }
}
