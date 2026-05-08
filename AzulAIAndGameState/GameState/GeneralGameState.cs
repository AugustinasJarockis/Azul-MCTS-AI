using AzulAIAndGameState.NewFolder;
using AzulBoardGame.Enums;

namespace AzulBoardGame.GameState
{
    public class GeneralGameState
    {
        public List<PlayerBoardState> PlayerBoardStates = [];
        public ITileBank TileBankState;
        public TilePlatesState TilePlatesState;
        public int PlayerCount => PlayerBoardStates.Count;
        public int CurrentPlayer { get; set; } = 0;
        public int NextRoundStartingPlayer { get; set; } = -1;
        
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

        public GeneralGameState((int, List<(List<List<int>>, List<(int, int)>, int, int)>, (List<int>, List<List<int>>), List<int>) listState) {
            CurrentPlayer = 0;
            NextRoundStartingPlayer = listState.Item1;
            foreach (var playerListState in listState.Item2) {
                PlayerBoardStates.Add(new(playerListState));
            }
            TilePlatesState = new(listState.Item3);
            TileBankState = new TileBank(listState.Item4);
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

        public (int, List<(List<List<int>>, List<(int, int)>, int, int)>, (List<int>, List<List<int>>), List<int>) GetListState() {
            List<(List<List<int>>, List<(int, int)>, int, int)> playerStatesList = [.. PlayerBoardStates.Select(p => p.GetListState())];
            if (CurrentPlayer == 1)
                playerStatesList.Reverse();

            return (
                (NextRoundStartingPlayer + CurrentPlayer) % 2,
                playerStatesList,
                TilePlatesState.GetListState(),
                TileBankState.GetListState()
                );
        }

        public float EstimatePositionValue(bool divBySum = false) {
            var currentPlayerState = PlayerBoardStates[CurrentPlayer];
            var oponentsState = PlayerBoardStates[(CurrentPlayer + 1) % 2];

            int earnedPoints = currentPlayerState.EstimateEarnedPoints();
            int oponentEarnedPoints = oponentsState.EstimateEarnedPoints();

            int totalPoints = Math.Max(0, currentPlayerState.Points + earnedPoints);
            int oponentTotalPoints = Math.Max(0, oponentsState.Points + oponentEarnedPoints);

            //return  (totalPoints - oponentTotalPoints) / (divBySum && (totalPoints + oponentTotalPoints) > 0 ? (totalPoints + oponentTotalPoints) : 1);
            return  (totalPoints - oponentTotalPoints) / (divBySum && (totalPoints + oponentTotalPoints) > 0 ? 10 : 1);
        }

        public bool IsMovePossible((byte plate, TileType type, byte row) move) {
            if (move.plate == 0 && !TilePlatesState.CenterTileTypes.Contains(move.type))
                return false;
            if (move.plate != 0 && !TilePlatesState.Plates[move.plate - 1].TileTypes.Contains(move.type))
                return false;
            if (move.row != 5 && !PlayerBoardStates[CurrentPlayer].CanBePlacedIntoRow(move.type, move.row))
                return false;
            return true;
        }

        public void StartNextRound() {
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
