namespace AzulBoardGame.PlayerBoard.PointCounter
{
    internal interface IPointCounter
    {
        public int Points { get; }
        public void UpdatePoints(int pointsChange);
    }
}
