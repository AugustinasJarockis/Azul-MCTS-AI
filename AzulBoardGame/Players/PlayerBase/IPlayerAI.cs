using AzulBoardGame.Enums;
using AzulBoardGame.GameState;

namespace AzulBoardGame.Players.PlayerBase
{
    internal interface IPlayerAI
    {
        public (byte, TileType, byte) ChooseMove(GeneralGameState gameState);
    }
}
