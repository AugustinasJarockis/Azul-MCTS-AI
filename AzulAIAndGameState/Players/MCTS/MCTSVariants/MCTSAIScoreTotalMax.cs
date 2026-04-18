using AzulBoardGame.Players.MCTS.StateEvaluators;

namespace AzulBoardGame.Players.MCTS.MCTSVariants
{
    internal class MCTSAIScoreTotalMax : MCTSAI
    {
        public MCTSAIScoreTotalMax()
            : base(
                  new GenericStateEvaluator(
                      GenericStateEvaluator.MaxScore,
                      GenericStateEvaluator.PointTotal,
                      GenericStateEvaluator.MaxPoints
                      )
                  ) { }
    }
}
