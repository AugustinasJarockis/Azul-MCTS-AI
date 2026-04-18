using AzulBoardGame.Players.MCTS.StateEvaluators;

namespace AzulBoardGame.Players.MCTS.MCTSVariants
{
    internal class MCTSAIScoreDiffMax : MCTSAI
    {
        public MCTSAIScoreDiffMax()
            : base(
                  new GenericStateEvaluator(
                      GenericStateEvaluator.MaxScore,
                      GenericStateEvaluator.PointDifference,
                      GenericStateEvaluator.MaxPoints
                      )
                  ) { }
    }
}
