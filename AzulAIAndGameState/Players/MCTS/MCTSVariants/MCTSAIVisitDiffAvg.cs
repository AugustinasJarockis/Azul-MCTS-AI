using AzulBoardGame.Players.MCTS.StateEvaluators;

namespace AzulBoardGame.Players.MCTS.MCTSVariants
{
    internal class MCTSAIVisitDiffAvg : MCTSAI
    {
        public MCTSAIVisitDiffAvg()
            : base(
                  new GenericStateEvaluator(
                      GenericStateEvaluator.MaxVisit,
                      GenericStateEvaluator.PointDifference,
                      GenericStateEvaluator.AveragePoints
                      )
                  ) { }
    }
}
