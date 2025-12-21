using AzulBoardGame.Enums;
using AzulBoardGame.GameTilePlates;
using AzulBoardGame.Players.MCTS.StateEvaluators;
using AzulBoardGame.Players.PlayerBase;

namespace AzulBoardGame.Players.MCTS
{
    internal class GameState {
        static private Random rnd = new(DateTime.Now.Microsecond * DateTime.Now.Millisecond);

        private readonly IStateEvaluator _stateEvaluator;

        private int seed = rnd.Next();
        private List<GameState> reachableStates = [];

        public (byte, TileType, byte)? moveMade;

        private List<(byte, TileType, byte)> possibleMoves = [];

        public int EndsReached { get; set; } = 0;
        public double Score { get; set; } = 0;

        private List<PlayerState<HeuristicStateDelver>> players = [];
        private DeterministicTileBank tileBank;
        private TilePlatesState tilePlates;

        private int playerOfInterest = 0; 
        private int playerCount => players.Count == 0 ? 4 : players.Count;
        private int plateCount => (playerCount) switch {
            2 => 5,
            3 => 7,
            4 => 9,
            _ => 5
        };
        public int CurrentPlayer { get; private set; } = 0;

        public GameState(int currentPlayer, IStateEvaluator stateEvaluator) {
            _stateEvaluator = stateEvaluator;
            
            tilePlates = new TilePlatesState(this, plateCount);
            tileBank = new DeterministicTileBank();
            tileBank.TileSelectionOutcomeSeed = seed;
            CurrentPlayer = currentPlayer;
            playerOfInterest = currentPlayer;

            for (int i = 0; i < playerCount; i++) {
                players.Add(new HeuristicStateDelver(tilePlates, tileBank));
            }

            possibleMoves = players[CurrentPlayer].GetPossibleMoves();
        }

        public GameState(
            DeterministicTileBank tileBank,
            TilePlates tilePlates,
            List<Player> players,
            int currentPlayer,
            int playerOfInterest,
            IStateEvaluator stateEvaluator,
            int? seed = null
            ) {
            _stateEvaluator = stateEvaluator;

            this.tilePlates = tilePlates.GetState(this);
            this.tileBank = tileBank.Copy();
            if (seed != null)
                this.tileBank.TileSelectionOutcomeSeed = (int)seed;
            CurrentPlayer = currentPlayer;
            this.playerOfInterest = playerOfInterest;

            foreach (var player in players) {
                this.players.Add(player.GetHeuristicStateDelver(this.tilePlates, this.tileBank));
            }

            possibleMoves = this.players[CurrentPlayer].GetPossibleMoves();
        }

        public GameState(
            DeterministicTileBank tileBank, 
            TilePlatesState tilePlates, 
            List<PlayerState<HeuristicStateDelver>> players, 
            int currentPlayer,
            int playerOfInterest,
            IStateEvaluator stateEvaluator,
            int? seed = null
            ) {
            _stateEvaluator = stateEvaluator;

            this.tilePlates = tilePlates.Copy(this);
            this.tileBank = tileBank.Copy();
            if (seed != null)
                this.tileBank.TileSelectionOutcomeSeed = (int)seed;
            CurrentPlayer = currentPlayer;
            this.playerOfInterest = playerOfInterest;

            foreach (var player in players) {
                this.players.Add(player.Copy(this.tilePlates, this.tileBank));
            }

            possibleMoves = players[CurrentPlayer].GetPossibleMoves();
        }

        public GameState Copy() {
            return new (tileBank, tilePlates, players, (CurrentPlayer + 1) % players.Count, playerOfInterest, _stateEvaluator, seed);
        }

        public GameState GetSyncWithManager(int playerCount, GameManager gameManager) {
            if (tilePlates.TotalTileCount == 0)
                return gameManager.GetState(playerOfInterest, _stateEvaluator);

            if (playerCount == 0)
                return this;

            var recentMove = gameManager.recentMoves[gameManager.PlayerCount - playerCount];

            if (recentMove != null
                && possibleMoves.Contains(((byte, TileType, byte))gameManager.recentMoves[gameManager.PlayerCount - playerCount]!)
            ) {
                var indexOfMove = possibleMoves.IndexOf(((byte, TileType, byte))recentMove);
                if (indexOfMove < reachableStates.Count) {
                    return reachableStates[indexOfMove].GetSyncWithManager(playerCount - 1, gameManager);
                }
            }
            return gameManager.GetState(playerOfInterest, _stateEvaluator);
        }

        public (byte, TileType, byte) GetBestMove() => _stateEvaluator.GetBestMove(possibleMoves, reachableStates);

        public void MakeRandomMove((byte, TileType, byte) move) {
            players[(CurrentPlayer + players.Count - 1) % players.Count].SelectTiles(move);
            moveMade = move;
            possibleMoves = players[CurrentPlayer].GetPossibleMoves();
        }

        public void MakeHeuristicMove() {
            moveMade = players[(CurrentPlayer + players.Count - 1) % players.Count].SelectTiles();
            possibleMoves = players[CurrentPlayer].GetPossibleMoves();
        }

        public void PlayOut(bool newMove) {
            //if (tilePlates.TotalTileCount == 0) { //TODO: Nekreipiama dėmesio į random visiškai dabar
            //    foreach (var player in players)
            //        player.CompleteRound();

            //    var tileTypes = tileBank.RefreshTiles(plateCount);
            //    tilePlates.RefreshPlates(tileTypes);
            //    CurrentPlayer = tilePlates.StartingPlayer;

            //    possibleMoves = players[CurrentPlayer].GetPossibleMoves();
            //}

            if (players.Any(p => p.HasFinished()) || tilePlates.TotalTileCount == 0) {
                foreach (var player in players)
                    player.CompleteRound();

                foreach (var player in players)
                    player.CalculateAdditionalPoints();
                
                Score = _stateEvaluator.GetEndPositionScore(playerOfInterest, players);
                EndsReached = 1;
                return;
            }

            if (reachableStates.Count == possibleMoves.Count) {
                reachableStates.MaxBy(
                    s => (s.Score + 20) / 100 + 1.414 * Math.Sqrt((2 * Math.Log(EndsReached)) / s.EndsReached)
                    )?.PlayOut(true);
            }
            else {
                var newState = Copy();
                if (newMove) {
                    newState.MakeRandomMove(possibleMoves[reachableStates.Count]);
                }
                else {
                    newState.MakeHeuristicMove();
                    int newStateMoveIndex = possibleMoves.IndexOf(((byte, TileType, byte))newState.moveMade!);
                    (possibleMoves[reachableStates.Count], possibleMoves[newStateMoveIndex]) = (possibleMoves[newStateMoveIndex], possibleMoves[reachableStates.Count]);
                }
                newState.PlayOut(false);
                reachableStates.Add(newState);
            }

            Score = _stateEvaluator.GetPositionScore(reachableStates);
            EndsReached++;
        }
    }
}
