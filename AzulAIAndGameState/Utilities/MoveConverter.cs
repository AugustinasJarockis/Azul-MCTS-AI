using AzulBoardGame.Enums;

namespace AzulAIAndGameState.NewFolder
{
    public static class MoveConverter
    {
        public static (byte, TileType, byte) MoveIntToTuple(int move) {
            return ((byte)(move / 30), (TileType)(move % 30 / 6 + 1), (byte)(move % 6));
        }
        public static int MoveTupleToInt((byte plate, TileType type, byte row) move) {
            return move.plate * 30 + (int)(move.type - 1) * 6 + move.row;
        }
    }
}
