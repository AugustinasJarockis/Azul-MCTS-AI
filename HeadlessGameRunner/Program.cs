using AzulAIAndGameState.NewFolder;
using AzulAIAndGameState.Players.MCTS_CNN;
using AzulBoardGame.Enums;
using AzulBoardGame.Extensions;
using AzulBoardGame.GameState;
using AzulBoardGame.Players.MCTS;
using AzulBoardGame.Players.MCTS.MCTSVariants;
using AzulBoardGame.Players.MCTS_CNN;
using AzulBoardGame.Players.PlayerBase;

var Player1AI = new MCTSAIScoreDiffAvg();
var Player2AI = new MCTSAIScoreDiffAvg();

var gameState = new GeneralGameState(2);

var Player1 = new HeadlessPlayer(gameState.PlayerBoardStates[0], Player1AI);
var Player2 = new HeadlessPlayer(gameState.PlayerBoardStates[1], Player2AI);

List<HeadlessPlayer> players = [Player1, Player2];
((MCTSAI)players[0].PlayerAI).timeAllotedMs = 100;
((MCTSAI)players[1].PlayerAI).timeAllotedMs = 100;

//Test(900, "RepeatedBestAgentTests.csv");
//RunTests();
//PlayGame();
//GenerateMoves("MoveDatabase.csv", 1000);
var trainer = new NetworkTrainer("MoveDatabase.csv", "trainingData.csv");
var model = new PolicyValueNetwork("Models/model92.nn");
trainer.Train(model, 10);

void GenerateMoves(string filename, int gameCount) {
    for (int i = 0; i < gameCount; i++) {
        Console.WriteLine("Playing game nr. " + i);
        List<List<int>> positions = [];
        
        while (!players.Any(p => p.HasFinished())) {
            var tileTypes = gameState.TileBankState.RefreshTiles(gameState.TilePlatesState.Plates.Count);
            gameState.TilePlatesState.RefreshPlates(tileTypes);


            while (gameState.TilePlatesState.TotalTileCount > 0) {
                positions.Add(gameState.GetListState().Flatten());

                players[gameState.CurrentPlayer].MakeMove(gameState);

                var move = ((byte, TileType, byte))(gameState.PlayerBoardStates[(gameState.CurrentPlayer + 1) % gameState.PlayerBoardStates.Count].MoveMade)!;
                int moveChosen = MoveConverter.MoveTupleToInt(move);
                positions[^1].Add(moveChosen);
            }

            foreach (HeadlessPlayer player in players)
                player.CompleteRound(gameState.TileBankState);
        }

        foreach (HeadlessPlayer player in players)
            player.CalculateAdditionalPoints();

        int score = players[0].Points - players[1].Points;

        for (int i2 = 0; i2 < positions.Count; i2++) {
            positions[i2].Add(score * (1 - 2 * (i2 % 2)));
        }

        WriteGamePositions(filename, positions);

        gameState.Reset();
    }
}

void RunTests() {
    //0.001s
    Test(100, "HeuristicTime0.001s.csv", 1);
    //0.002s
    Test(100, "HeuristicTime0.002s.csv", 2);
    //0.005s
    Test(100, "HeuristicTime0.005s.csv", 5);
    //0.01s
    Test(100, "HeuristicTime0.01s.csv", 10);
    //0.025s
    Test(100, "HeuristicTime0.025s.csv", 25);
    //0.05s
    Test(100, "HeuristicTime0.05s.csv", 50);
    //0.1s
    Test(100, "HeuristicTime0.1s.csv", 100);
    //0.2s
    Test(100, "HeuristicTime0.2s.csv", 200);
    //0.3s
    Test(100, "HeuristicTime0.3s.csv", 300);
    //0.4s
    Test(100, "HeuristicTime0.4s.csv", 400);
    //0.5s
    Test(100, "HeuristicTime0.5s.csv", 500);
    //0.6s
    Test(100, "HeuristicTime0.6s.csv", 600);
    //0.7s
    Test(100, "HeuristicTime0.7s.csv", 700);
    //0.8s
    Test(100, "HeuristicTime0.8s.csv", 800);
    //0.9s
    Test(100, "HeuristicTime0.9s.csv", 900);
    //1s
    Test(100, "HeuristicTime1s.csv", 1000);
}

void Test(int count, string filename, int player1TimeMs = -1) {
    File.Create(filename);

    for (int i = 0; i < count; i++) {
        int startingPlayer = i / ((count + players.Count - 1) / players.Count);
        gameState.NextRoundStartingPlayer = startingPlayer;
        gameState.CurrentPlayer = startingPlayer;
        if (player1TimeMs > 0) {
            ((MCTSAI)players[0].PlayerAI).timeAllotedMs = player1TimeMs;
        }
        PlayGame();
        WriteResults(filename, startingPlayer);
        gameState.Reset();
    }
}

void WriteResults(string filename, int startingPlayer) {
    string textToAppend = "";
    string delimiter = "; ";
    textToAppend += players.Count + delimiter;
    textToAppend += startingPlayer + delimiter;
    foreach (var player in players) {
        textToAppend += player.PlayerAI.ToString() + delimiter;
        textToAppend += player.Points + delimiter;
    }

    textToAppend += players.IndexOf(players.MaxBy(p => p.Points)).ToString() + '\n';

    File.AppendAllText(filename, textToAppend);
}

void WriteGamePositions(string filename, List<List<int>> positions) {
    foreach (var position in positions) {
        string textToAppend = "";
        string delimiter = ";";

        for (int i = 0; i < position.Count; i++) {
            textToAppend += position[i] + delimiter;
        }

        textToAppend += '\n';
        File.AppendAllText(filename, textToAppend);
    }
}

void PlayGame() {
    while (!players.Any(p => p.HasFinished())) {
        var tileTypes = gameState.TileBankState.RefreshTiles(gameState.TilePlatesState.Plates.Count);
        gameState.TilePlatesState.RefreshPlates(tileTypes);

        while (gameState.TilePlatesState.TotalTileCount > 0) {
            players[gameState.CurrentPlayer].MakeMove(gameState);
            gameState.CurrentPlayer = (gameState.CurrentPlayer + 1) % players.Count;
        }

        foreach (HeadlessPlayer player in players)
            player.CompleteRound(gameState.TileBankState);
    }

    foreach (HeadlessPlayer player in players)
        player.CalculateAdditionalPoints();
}