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
            var state = gameState.GetListState().Flatten().Select(x => (float)x).ToArray().ToTensor([1, 134]);

            var (policy, _) = _model.Call(state);
            policy = policy[0].softmax(0);

            int move = (int)policy.argmax().item<long>();

            //List<float> moveArray = [];
            //for (int i = 0; i < 180; i++) {
            //    moveArray.Add(policy.view(-1)[i].item<float>());
            //}
            
            while (!IsMovePossible(move, gameState)) {
                policy.view(-1)[move] = -1;
                move = (int)policy.argmax().item<long>();
            }

            return MoveConverter.MoveIntToTuple(move);
        }

        private bool IsMovePossible(int move, GeneralGameState gameState) {
            (byte plate, TileType type, byte row) = MoveConverter.MoveIntToTuple(move);
            
            if (plate == 0 && !gameState.TilePlatesState.CenterTileTypes.Contains(type)) {
                return false;
            }

            if (plate != 0 && !gameState.TilePlatesState.Plates[plate - 1].TileTypes.Contains(type)) {
                return false;
            }

            if (row != 5 && !gameState.PlayerBoardStates[gameState.CurrentPlayer].CanBePlacedIntoRow(type, row))
                return false;

            return true;
        }
    }
}
