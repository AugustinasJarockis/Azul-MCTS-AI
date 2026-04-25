using AzulAIAndGameState.NewFolder;
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
        private PolicyValueNetwork _model = new();
        public PolicyNetworkAI(string modelPath) {
            _model.load(modelPath);
        }
        public (byte, TileType, byte) ChooseMove(GeneralGameState gameState) {
            var state = gameState.GetListState().Flatten().Select(x => (float)x).ToArray().ToTensor([1, 121]);

            var (policy, _) = _model.Call(state);
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
