using AzulBoardGame.Players.MCTS.StateEvaluators;

namespace AzulBoardGame.Players.MCTS.MCTSVariants
{
    internal class MCTSAIScoreTotalAvg : MCTSAI
    {
        public MCTSAIScoreTotalAvg()
            : base(
                  new GenericStateEvaluator(
                      GenericStateEvaluator.MaxScore,
                      GenericStateEvaluator.PointTotal,
                      GenericStateEvaluator.AveragePoints
                      )
                  ) { }
    }
}
