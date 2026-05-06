using AzulAIAndGameState.NewFolder;
using AzulAIAndGameState.Players.MCTS_CNN;
using AzulAIAndGameState.Players.MCTS_NN;
using AzulAIAndGameState.Players.MCTS_NN.Networks;
using AzulAIAndGameState.Players.MiniMax;
using AzulBoardGame.Enums;
using AzulBoardGame.Extensions;
using AzulBoardGame.GameState;
using AzulBoardGame.Players;
using AzulBoardGame.Players.MCTS;
using AzulBoardGame.Players.MCTS.MCTSVariants;
using AzulBoardGame.Players.MCTS_CNN;
using AzulBoardGame.Players.PlayerBase;

//Test(900, "RepeatedBestAgentTests.csv");
//RunTests();
//PlayGame();

//GenerateMoves("GoodMoveDatabase.csv", 10000);

//Parallel.For(0, 100, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
//    i => new GameSetup().GenerateMoves("GoodMoveDatabase" + i + ".csv", 100));

//Console.WriteLine("All threads finished");

//var trainer = new NetworkTrainer("GoodMoveDatabaseFull.csv", "TrainingDataWithSmallConvValue.v0.csv");
//var model = new PolicyValueNetwork("Models/hmodel48.nn");
//var model = new ConvolutionalPolicyNetwork();
//var model = new SmallConvPolicyNetwork();
//var model = new SmallConvValueNetwork();
//var model = new PolicyNetwork();
//var model = new ValueNetwork();
//trainer.TrainLegalAndNoProcessing(model, 50);
//trainer.Train(model, 100, 512);
//trainer.TrainValue(model, 100, 512);

new GameSetup().Test(10000, "TestReinforcementLearning.v1.csv");
//((MCTSnNNAI)players[0].PlayerAI).SaveModel("Models/RL4.nn");

//MergeFiles();
//void MergeFiles()
//{
//    for (int i = 0; i < 100; i++)
//    {
//        string contents = File.ReadAllText("GoodMoveDatabase" + i + ".csv");
//        File.AppendAllText("GoodMoveDatabaseFull.csv", contents);
//    }
//}

public class GameSetup
{
    //public MCTSnNNAI Player1AI = new ("Models/hmodel48.nn", trainingOn: true);
    public IPlayerAI Player1AI;
    public IPlayerAI Player2AI;

    public GeneralGameState gameState = new (2);

    public HeadlessPlayer Player1;
    public HeadlessPlayer Player2;

    public List<HeadlessPlayer> players;

    public GameSetup() {
        var policyNetwork = new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn");
        var valueNetwork = new SmallConvValueNetwork("Models/smallconvvaluemodel71.v0.nn");

        //Player1AI = new HeuristicAI();
        //Player1AI = new MCTSAIScoreDiffAvg();
        Player1AI = new MCTSnNNAI(policyNetwork, valueNetwork, trainingOn: false);
        //Player1AI = new PolicyNetworkAI(new ConvolutionalPolicyNetwork("Models/convmodel9.v0.nn"));
        //Player2AI = new PolicyNetworkAI(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"));
        //Player2AI = new PolicyNetworkAI(new PolicyNetwork("Models/fullmodel10.v3.nn"));
        //Player2AI = new MCTSnNNAI("Models/fullmodel10.v3.nn", "Models/valuemodel21.v1.nn", trainingOn: false);
        Player2AI = new MCTSnNNAI(policyNetwork, valueNetwork, trainingOn: false);
        //Player2AI = new MCTSnCustomEval("Models/fullmodel10.v3.nn", trainingOn: false);
        //Player2AI = new MinimaxAI();
        Player1 = new(gameState.PlayerBoardStates[0], Player1AI);
        Player2 = new(gameState.PlayerBoardStates[1], Player2AI);
    
        players = [Player1, Player2];


        //((MCTSAI)players[0].PlayerAI).timeAllotedMs = 100;
        //((MCTSAI)players[1].PlayerAI).timeAllotedMs = 100;
    }

    public void GenerateMoves(string filename, int gameCount) {
        for (int i = 0; i < gameCount; i++) {
            Console.WriteLine("Playing game nr. " + i);
            List<List<int>> positions = [];

            while (!players.Any(p => p.HasFinished())) {
                var tileTypes = gameState.TileBankState.RefreshTiles(gameState.TilePlatesState.Plates.Count);
                gameState.TilePlatesState.RefreshPlates(tileTypes);


                while (gameState.TilePlatesState.TotalTileCount > 0) {
                    positions.Add(gameState.GetListState().Flatten());
                    int[] possibleMoveArray = new int[180];
                    var possibleMoves = gameState.PlayerBoardStates[gameState.CurrentPlayer].GetPossibleMoves(gameState.TilePlatesState);
                    for (int i2 = 0; i2 < 180; i2++) {
                        if (possibleMoves.Contains(MoveConverter.MoveIntToTuple(i2))) {
                            possibleMoveArray[i2] = 1;
                        }
                    }
                    positions[^1].AddRange(possibleMoveArray);

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
    public void WriteGamePositions(string filename, List<List<int>> positions) {
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

    public void RunTests() {
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

    public void Test(int count, string filename, int player1TimeMs = -1) {
        for (int i = 0; i < count; i++) {
            Console.WriteLine("Playing game nr. " + i);
            int startingPlayer = i / ((count + players.Count - 1) / players.Count);
            gameState.NextRoundStartingPlayer = startingPlayer;
            gameState.CurrentPlayer = startingPlayer;
            if (player1TimeMs > 0) {
                ((MCTSAI)players[0].PlayerAI).timeAllotedMs = player1TimeMs;
            }
            PlayGame();
            WriteResults(filename, startingPlayer);
            ((MCTSnNNAI)Player2AI).SaveModel("Models/RL/RLPolicy.v0.nn", "Models/RL/RLValue.v0.nn");
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

        Console.WriteLine("Writing to file");
        File.AppendAllText(filename, textToAppend);
    }

    void PlayGame() {
        while (!players.Any(p => p.HasFinished())) {
            var tileTypes = gameState.TileBankState.RefreshTiles(gameState.TilePlatesState.Plates.Count);
            gameState.TilePlatesState.RefreshPlates(tileTypes);

            while (gameState.TilePlatesState.TotalTileCount > 0) {
                players[gameState.CurrentPlayer].MakeMove(gameState);
            }

            foreach (HeadlessPlayer player in players)
                player.CompleteRound(gameState.TileBankState);
        }

        foreach (HeadlessPlayer player in players)
            player.CalculateAdditionalPoints();
    }
}