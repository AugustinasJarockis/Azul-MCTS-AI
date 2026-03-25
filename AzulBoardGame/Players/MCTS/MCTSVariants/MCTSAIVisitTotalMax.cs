using AzulBoardGame.Players.MCTS.StateEvaluators;

namespace AzulBoardGame.Players.MCTS.MCTSVariants
{
    internal class MCTSAIVisitTotalMax : MCTSAI
    {
        public MCTSAIVisitTotalMax()
            : base(
                  new GenericStateEvaluator(
                      GenericStateEvaluator.MaxVisit,
                      GenericStateEvaluator.PointTotal,
                      GenericStateEvaluator.MaxPoints
                      )
                  ) { }
    }
}
