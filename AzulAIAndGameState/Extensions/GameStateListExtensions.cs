namespace AzulBoardGame.Extensions
{
    public static class GameStateListExtensions
    {
        public static List<int> Flatten(this (int, List<(List<List<int>>, List<(int, int)>, int, int)>, (List<int>, List<List<int>>), List<int>) state) {
            List<int> result = [state.Item1];

            foreach (var item in state.Item2) {

                foreach (var rowState in item.Item1) {
                    result.AddRange(rowState);
                }

                foreach (var rowState in item.Item2) {
                    result.Add(rowState.Item1);
                    result.Add(rowState.Item2);
                }

                result.Add(item.Item3);
                result.Add(item.Item4);
            }

            result.AddRange(state.Item3.Item1);
            foreach (var item in state.Item3.Item2) {
                result.AddRange(item);
            }

            result.AddRange(state.Item4);

            return result;
        }

        public static (int, List<(List<List<int>>, List<(int, int)>, int, int)>, (List<int>, List<List<int>>), List<int>) ExpandToGameState(this List<int> stateList) {
            int nextRoundStartingPlayer = stateList[0];
            List<(List<List<int>>, List<(int, int)>, int, int)> playerBoardstates = [];

            // Player states
            for (int i = 0; i < 2; i++) {
                var playerStateList = stateList.Skip(1 + i * 37).Take(37);

                List<List<int>> gridStateList = [];
                for (int i2 = 0; i2 < 5; i2++) {
                    gridStateList.Add([..playerStateList.Skip(i * 5).Take(5)]);
                }

                List<(int, int)> rowStates = [];
                List<int> rowStatesList = [..playerStateList.Skip(25).Take(10)];
                for (int i2 = 0; i2 < 5; i2++) {
                    rowStates.Add((rowStatesList[i * 2], rowStatesList[i * 2 + 1]));
                }

                int processingLineState = playerStateList.SkipLast(1).Last();
                int playerPoints = playerStateList.Last();

                playerBoardstates.Add((gridStateList, rowStates, processingLineState, playerPoints));
            }

            // Tile plates state
            List<int> centerTiles = [.. stateList.Skip(75).Take(16)];

            List<List<int>> platesState = [];
            for (int i = 0; i < 5; i++) {
                platesState.Add([.. stateList.Skip(91 + i * 4).Take(4)]);
            }

            // Tile bank state
            List<int> tileBankState = [..stateList.TakeLast(10)];

            return (nextRoundStartingPlayer, playerBoardstates, (centerTiles, platesState), tileBankState);
        }
    }
}
