using AzulBoardGame.GameTilePlates;
using AzulBoardGame.Players.MCTS.StateEvaluators;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AzulBoardGame.Players.MCTS.MCTSVariants
{
    internal class MCTSAIVisitTotalMax : MCTSAI {
        public MCTSAIVisitTotalMax (
            GameManager gameManager,
            Canvas mainCanvas,
            ScaleTransform scaleTransform,
            TranslateTransform translateTransform,
            Action notifyAboutCompletion,
            TilePlates tilePlates,
            ITileBank tileBank,
            string name,
            Brush nameColour,
            Key keyToFocus,
            double xPos,
            double yPos,
            double size,
            bool pauseBetweenChoices = false
            )
            : base(
                  new GenericStateEvaluator(
                      GenericStateEvaluator.MaxVisit,
                      GenericStateEvaluator.PointTotal,
                      GenericStateEvaluator.MaxPoints
                      ),
                  gameManager,
                  mainCanvas, 
                  scaleTransform, 
                  translateTransform, 
                  notifyAboutCompletion, 
                  tilePlates, 
                  tileBank, 
                  name, 
                  nameColour, 
                  keyToFocus, 
                  xPos, 
                  yPos, 
                  size,
                  pauseBetweenChoices
                  ) 
            {}
    }
}
