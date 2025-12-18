namespace AzulBoardGame.PlayerBoard.PointCounter
{
    internal class InvisiblePointCounter : IPointCounter
    {
        public int Points { get; private set; } = 0;
        public InvisiblePointCounter() { }
        public InvisiblePointCounter(int points) { 
            Points = points;
        }
        public InvisiblePointCounter Copy() {
            return new InvisiblePointCounter(Points);
        }

        public void UpdatePoints(int pointsChange) {
            Points += pointsChange;
            Points = Math.Max(Points, 0);
        }
    }
}
