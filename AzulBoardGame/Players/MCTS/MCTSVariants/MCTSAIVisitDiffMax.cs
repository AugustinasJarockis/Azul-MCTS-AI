using AzulBoardGame.Players.MCTS.StateEvaluators;

namespace AzulBoardGame.Players.MCTS.MCTSVariants
{
    internal class MCTSAIVisitDiffMax : MCTSAI
    {
        public MCTSAIVisitDiffMax()
            : base(
                  new GenericStateEvaluator(
                      GenericStateEvaluator.MaxVisit,
                      GenericStateEvaluator.PointDifference,
                      GenericStateEvaluator.MaxPoints
                      )
                  ) { }
    }
}
