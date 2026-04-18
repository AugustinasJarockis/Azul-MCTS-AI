using AzulBoardGame.Players.MCTS.StateEvaluators;

namespace AzulBoardGame.Players.MCTS.MCTSVariants
{
    internal class MCTSAIVisitTotalAvg : MCTSAI
    {
        public MCTSAIVisitTotalAvg()
            : base(
                  new GenericStateEvaluator(
                      GenericStateEvaluator.MaxVisit,
                      GenericStateEvaluator.PointTotal,
                      GenericStateEvaluator.AveragePoints
                      )
                  ) { }
    }
}
