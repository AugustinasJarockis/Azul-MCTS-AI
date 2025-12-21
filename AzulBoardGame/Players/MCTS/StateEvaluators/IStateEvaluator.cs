using AzulBoardGame.Enums;
using AzulBoardGame.Players.PlayerBase;

namespace AzulBoardGame.Players.MCTS.StateEvaluators
{
    internal interface IStateEvaluator
    {
        public (byte, TileType, byte) GetBestMove(List<(byte, TileType, byte)> possibleMoves, List<GameState> reachableStates);
        public double GetPositionScore(List<GameState> reachableStates);
        public double GetEndPositionScore(int playerOfInterest, List<PlayerState<HeuristicStateDelver>> players);
    }
}
