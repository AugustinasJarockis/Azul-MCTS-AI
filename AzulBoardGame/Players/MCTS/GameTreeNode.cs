using AzulBoardGame.Enums;
using AzulBoardGame.GameState;
using AzulBoardGame.Players.MCTS.StateEvaluators;
using AzulBoardGame.Players.PlayerBase;

namespace AzulBoardGame.Players.MCTS
{
    internal class GameTreeNode
    {
        static private Random rnd = new(DateTime.Now.Microsecond * DateTime.Now.Millisecond);

        private readonly IStateEvaluator _stateEvaluator;

        private List<GameTreeNode> reachableStates = [];

        public (byte, TileType, byte)? moveMade;

        private List<(byte, TileType, byte)> possibleMoves = [];

        public int EndsReached { get; set; } = 0;
        public double Score { get; set; } = 0;

        private List<IPlayerAI> players = [];
        private GeneralGameState _gameState;

        private int playerOfInterest = 0;

        public GameTreeNode(GeneralGameState gameState, IStateEvaluator stateEvaluator, int playerOfInterest) {
            _stateEvaluator = stateEvaluator;
            _gameState = gameState;
            this.playerOfInterest = playerOfInterest;

            for (int i = 0; i < _gameState.PlayerCount; i++) {
                players.Add(new HeuristicAI());
            }

            possibleMoves = gameState.PlayerBoardStates[gameState.CurrentPlayer].GetPossibleMoves(gameState.TilePlatesState);
        }

        public GameTreeNode(GeneralGameState gameState, IStateEvaluator stateEvaluator, int playerOfInterest, List<IPlayerAI> players) {
            _stateEvaluator = stateEvaluator;
            _gameState = gameState;
            this.playerOfInterest = playerOfInterest;
            this.players = players;

            possibleMoves = gameState.PlayerBoardStates[gameState.CurrentPlayer].GetPossibleMoves(gameState.TilePlatesState);
        }

        public GameTreeNode GetSyncWithManager(int playerCount, GeneralGameState currentGameState) {
            if (_gameState.TilePlatesState.TotalTileCount == 0)
                return new(currentGameState.Copy(), _stateEvaluator, playerOfInterest);

            if (playerCount == 0)
                return this;

            var playerToMakeAMove = (currentGameState.PlayerCount - playerCount + _gameState.CurrentPlayer) % currentGameState.PlayerCount;
            var recentMove = currentGameState.PlayerBoardStates[playerToMakeAMove].MoveMade;

            if (recentMove != null
                && possibleMoves.Contains(((byte, TileType, byte))recentMove!)
            ) {
                var indexOfMove = possibleMoves.IndexOf(((byte, TileType, byte))recentMove);
                if (indexOfMove < reachableStates.Count) {
                    return reachableStates[indexOfMove].GetSyncWithManager(playerCount - 1, currentGameState);
                }
            }
            return new(currentGameState.Copy(), _stateEvaluator, playerOfInterest);
        }

        public (byte, TileType, byte) GetBestMove() => _stateEvaluator.GetBestMove(possibleMoves, reachableStates);

        public void MakeRandomMove((byte, TileType, byte) move) {
            _gameState.MakeMove(move);
            moveMade = move;
            possibleMoves = _gameState.PlayerBoardStates[_gameState.CurrentPlayer].GetPossibleMoves(_gameState.TilePlatesState); //TODO: seems wrong here
        }

        public void MakeHeuristicMove() {
            moveMade = players[(_gameState.CurrentPlayer + 1) % players.Count].ChooseMove(_gameState);
            _gameState.MakeMove(((byte, TileType, byte))moveMade);
            possibleMoves = _gameState.PlayerBoardStates[_gameState.CurrentPlayer].GetPossibleMoves(_gameState.TilePlatesState);
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

            if (_gameState.PlayerBoardStates.Any(p => p.HasFinished()) || _gameState.TilePlatesState.TotalTileCount == 0) {
                foreach (var player in _gameState.PlayerBoardStates)
                    player.CompleteRound(_gameState.TileBankState);

                foreach (var player in _gameState.PlayerBoardStates)
                    player.CalculateAdditionalPoints();

                Score = _stateEvaluator.GetEndPositionScore(playerOfInterest, _gameState.PlayerBoardStates);
                EndsReached = 1;
                return;
            }

            if (reachableStates.Count == possibleMoves.Count) {
                reachableStates.MaxBy(
                    s => (s.Score + 20) / 100 + 1.414 * Math.Sqrt((2 * Math.Log(EndsReached)) / s.EndsReached)
                    )?.PlayOut(true);
            }
            else {
                var newState = new GameTreeNode(_gameState.Copy(), _stateEvaluator, playerOfInterest);
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
