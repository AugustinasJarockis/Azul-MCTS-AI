using AzulBoardGame.Players.MCTS.StateEvaluators;

namespace AzulBoardGame.Players.MCTS.MCTSVariants
{
    public class MCTSAIScoreDiffAvg : MCTSAI
    {
        public MCTSAIScoreDiffAvg()
            : base(
                  new GenericStateEvaluator(
                      GenericStateEvaluator.MaxScore,
                      GenericStateEvaluator.PointDifference,
                      GenericStateEvaluator.AveragePoints
                      )
                  ) { }
    }
}
