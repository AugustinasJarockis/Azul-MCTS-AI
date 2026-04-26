using AzulAIAndGameState.NewFolder;
using AzulAIAndGameState.Players.MCTS_NN.Networks;
using AzulBoardGame.Enums;
using AzulBoardGame.Extensions;
using AzulBoardGame.GameState;
using AzulBoardGame.Players.MCTS_CNN;
using AzulBoardGame.Players.PlayerBase;
using TorchSharp;

namespace AzulAIAndGameState.Players.MCTS_CNN
{
    public class PolicyNetworkAI : IPlayerAI
    {
        private PolicyNetwork _model = new();
        public PolicyNetworkAI(string modelPath) {
            _model.load(modelPath);
        }
        public (byte, TileType, byte) ChooseMove(GeneralGameState gameState) {
            var stateList = gameState.GetListState().Flatten().Select(x => (float)x).ToList();
            float[] possibleMoveArray = new float[180];
            var possibleMoves = gameState.PlayerBoardStates[gameState.CurrentPlayer].GetPossibleMoves(gameState.TilePlatesState);
            for (int i2 = 0; i2 < 180; i2++)
            {
                if (possibleMoves.Contains(MoveConverter.MoveIntToTuple(i2)))
                {
                    possibleMoveArray[i2] = 1;
                }
            }
            stateList.AddRange(possibleMoveArray);
            var state = stateList.ToArray().ToTensor([1, 301]);

            var policy = _model.Call(state);
            policy = policy[0].softmax(0);

            int move = (int)policy.argmax().item<long>();

            //List<float> moveArray = [];
            //for (int i = 0; i < 180; i++) {
            //    moveArray.Add(policy.view(-1)[i].item<float>());
            //}
            
            while (!gameState.IsMovePossible(MoveConverter.MoveIntToTuple(move))) {
                policy.view(-1)[move] = -1;
                move = (int)policy.argmax().item<long>();
            }

            return MoveConverter.MoveIntToTuple(move);
        }
    }
}
