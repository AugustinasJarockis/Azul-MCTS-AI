using AzulBoardGame.Enums;

namespace AzulBoardGame.GameState
{
    internal class GeneralGameState
    {
        public List<PlayerBoardState> PlayerBoardStates = [];
        public ITileBank TileBankState;
        public TilePlatesState TilePlatesState;
        public int PlayerCount => PlayerBoardStates.Count;
        public int CurrentPlayer { get; set; } = 0;

        public int NextRoundStartingPlayer { get; set; } = 0;
        
        public GeneralGameState(int playerCount) {
            TileBankState = new TileBank();
            
            for (int i = 0; i < playerCount; i++) {
                PlayerBoardStates.Add(new());
            }

            int plateCount = (playerCount) switch {
                2 => 5,
                3 => 7,
                4 => 9,
                _ => 5
            };

            TilePlatesState = new(plateCount);
        }

        public GeneralGameState(
            List<PlayerBoardState> playerBoardStates, 
            ITileBank tileBankState, 
            TilePlatesState tilePlatesState, 
            int currentPlayer
        ) {
            PlayerBoardStates = playerBoardStates;
            TileBankState = tileBankState;
            TilePlatesState = tilePlatesState;
            CurrentPlayer = currentPlayer;
        }

        public GeneralGameState Copy() {
            List<PlayerBoardState> playerStatesCopies = [..PlayerBoardStates.Select(p => p.Copy())];
            return new(playerStatesCopies, TileBankState.Copy(), TilePlatesState.Copy(), CurrentPlayer);
        }

        public void Reset() {
            foreach (var playerBoard in PlayerBoardStates) {
                playerBoard.Reset();
            }
            TileBankState.Reset();
            TilePlatesState.Reset();
        }

        public void StartNextRound() { //TODO: sync UI to state
            CurrentPlayer = NextRoundStartingPlayer;
            foreach (var playerBoard in PlayerBoardStates) {
                playerBoard.CompleteRound(TileBankState);
            }
            List<TileType> nextRoundTiles = TileBankState.RefreshTiles(TilePlatesState.Plates.Count);
            TilePlatesState.RefreshPlates(nextRoundTiles);
        }

        public void MakeMove((byte plate, TileType tileType, byte row) move) {
            List<TileType> selectedTiles = TilePlatesState.SelectTiles(move.tileType, move.plate);
            if (selectedTiles[^1] == TileType.First)
                NextRoundStartingPlayer = CurrentPlayer;
            PlayerBoardStates[CurrentPlayer].ManageSelectedTiles(selectedTiles, TileBankState);
            PlayerBoardStates[CurrentPlayer].SelectRow(move.row, TileBankState);
            CurrentPlayer = (CurrentPlayer + 1) % PlayerCount;
        }
    }
}
