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
//    i => new GameSetup().GenerateMoves("EvenBiggerMoveDatabase" + i + ".csv", 100));

//Console.WriteLine("All threads finished");

//var trainer = new NetworkTrainer("GoodMoveDatabaseFull.csv", "FixedConvTraining.v0.csv");
//var trainer = new NetworkTrainer("HUGEMoveDatabaseFull.csv", "SmallLinearVallueOnHUGE.v0.csv");
//var model = new PolicyValueNetwork("Models/hmodel48.nn");
//var model = new ConvolutionalPolicyNetwork();
//var model = new SmallConvPolicyNetwork();
//var model = new SmallConvValueNetwork();
//var model = new PolicyNetwork();
//var model = new SmallLinearPolicyNetwork();
//var model = new ValueNetwork();
//var model = new SmallLinearValueNetwork();
//trainer.TrainLegalAndNoProcessing(model, 50);
//trainer.Train(model, 50, "bigconvonhugemodel", 0, 512);
//trainer.TrainValue(model, 50, "smalllinearvalueonhugemodel", 0, 512);

new GameSetup().Test(100, "NerfedMinimaxVSMCTSnCustomEval.csv");

//new GameSetup().Test(10000, "TestReinforcementLearning.v2.csv");
//new GameSetup().Test(100, "TestHugeModelGainsWithCustomEval.v0.csv");
//((MCTSnNNAI)players[0].PlayerAI).SaveModel("Models/RL4.nn");

//new GameSetup().RunTests();
//new GameSetup().RerunNetworkTests();
//new GameSetup().RunAdditionalNetworkTests();
//new GameSetup().RunTestsWithBigLinear();
//new GameSetup().RunNetworkTestsWithMCTSandCustomValueFunction();
//new GameSetup().RerunNetworkTestsWithNewModels();
//new GameSetup().RunNewModelNetworkTestsWithMCTSandCustomValueFunction();
//new GameSetup().RunTestsWithNewSmallLinear();
//new GameSetup().RunMCTSnNNAITestsWithNewValueNetwork();

//MergeFiles();
//void MergeFiles()
//{
//    string old_contents = File.ReadAllText("GoodMoveDatabaseFull.csv");
//    File.AppendAllText("HUGEMoveDatabaseFull.csv", old_contents);
//    for (int i = 0; i < 100; i++)
//    {
//        string contents = File.ReadAllText("BiggerMoveDatabase" + i + ".csv");
//        File.AppendAllText("HUGEMoveDatabaseFull.csv", contents);
//    }
//    for (int i = 0; i < 100; i++)
//    {
//        string contents = File.ReadAllText("EvenBiggerMoveDatabase" + i + ".csv");
//        File.AppendAllText("HUGEMoveDatabaseFull.csv", contents);
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
        //var policyNetwork = new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn");
        //var valueNetwork = new SmallConvValueNetwork("Models/smallconvvaluemodel71.v0.nn");

        //Player1AI = new HeuristicAI();
        //Player1AI = new MCTSAIScoreDiffAvg();
        //Player2AI = new MCTSAIScoreDiffAvg();
        //Player1AI = new MCTSnNNAI(policyNetwork, valueNetwork, trainingOn: true);
        //Player1AI = new PolicyNetworkAI(new ConvolutionalPolicyNetwork("Models/convmodel9.v0.nn"));
        //Player2AI = new PolicyNetworkAI(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"));
        //Player2AI = new PolicyNetworkAI(new PolicyNetwork("Models/fullmodel10.v3.nn"));
        //Player1AI = new MCTSnCustomEval(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"));
        Player1AI = new MCTSnCustomEval(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        //Player2AI = new MCTSnNNAI("Models/fullmodel10.v3.nn", "Models/valuemodel21.v1.nn", trainingOn: false);
        //Player2AI = new MCTSnNNAI(policyNetwork, valueNetwork, trainingOn: true);
        //Player2AI = new MCTSnCustomEval("Models/fullmodel10.v3.nn", trainingOn: false);
        Player2AI = new MinimaxAI();
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
        //Test how different models work
        Console.WriteLine("Testing batch nr. 1");
        Player1.PlayerAI = new PolicyNetworkAI(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"));
        Player2.PlayerAI = new PolicyNetworkAI(new ConvolutionalPolicyNetwork("Models/convmodel9.v0.nn"));
        Test(100, "FinalTests/SmallVSBigConvModel.csv");

        Console.WriteLine("Testing batch nr. 2");
        Player1.PlayerAI = new MCTSnPolicy(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"));
        Player2.PlayerAI = new MCTSnPolicy(new ConvolutionalPolicyNetwork("Models/convmodel9.v0.nn"));
        Test(100, "FinalTests/SmallVSBigConvModelWithMCTS.csv");

        Console.WriteLine("Testing batch nr. 3");
        Player1.PlayerAI = new PolicyNetworkAI(new PolicyNetwork("Models/fullmodel10.v3.nn"));
        Player2.PlayerAI = new PolicyNetworkAI(new SmallLinearPolicyNetwork("Models/smalllinearmodel5.v0.nn"));
        Test(100, "FinalTests/SmallVSBigLinearModel.csv");

        Console.WriteLine("Testing batch nr. 4");
        Player1.PlayerAI = new MCTSnPolicy(new PolicyNetwork("Models/fullmodel10.v3.nn"));
        Player2.PlayerAI = new MCTSnPolicy(new SmallLinearPolicyNetwork("Models/smalllinearmodel5.v0.nn"));
        Test(100, "FinalTests/SmallVSBigLinearModelWithMCTS.csv");

        Console.WriteLine("Testing batch nr. 5");
        Player1.PlayerAI = new PolicyNetworkAI(new PolicyNetwork("Models/fullmodel10.v3.nn"));
        Player2.PlayerAI = new PolicyNetworkAI(new ConvolutionalPolicyNetwork("Models/convmodel9.v0.nn"));
        Test(100, "FinalTests/ConvVSLinearBigModel.csv");

        Console.WriteLine("Testing batch nr. 6");
        Player1.PlayerAI = new MCTSnPolicy(new PolicyNetwork("Models/fullmodel10.v3.nn"));
        Player2.PlayerAI = new MCTSnPolicy(new ConvolutionalPolicyNetwork("Models/convmodel9.v0.nn"));
        Test(100, "FinalTests/ConvVSLinearBigModelWithMCTS.csv");

        Console.WriteLine("Testing batch nr. 7");
        Player1.PlayerAI = new PolicyNetworkAI(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"));
        Player2.PlayerAI = new PolicyNetworkAI(new SmallLinearPolicyNetwork("Models/smalllinearmodel5.v0.nn"));
        Test(100, "FinalTests/ConvVSLinearSmallModel.csv");

        Console.WriteLine("Testing batch nr. 8");
        Player1.PlayerAI = new MCTSnPolicy(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"));
        Player2.PlayerAI = new MCTSnPolicy(new SmallLinearPolicyNetwork("Models/smalllinearmodel5.v0.nn"));
        Test(100, "FinalTests/ConvVSLinearSmallModelWithMCTS.csv");

        // Strategy comparisons

        // Heuristic vs oponents

        Console.WriteLine("Testing batch nr. 9");
        Player1.PlayerAI = new HeuristicAI();
        Player2.PlayerAI = new MinimaxAI();
        Test(100, "FinalTests/HeuristicVSMinimaxAI.csv");

        Console.WriteLine("Testing batch nr. 10");
        Player1.PlayerAI = new HeuristicAI();
        Player2.PlayerAI = new PolicyNetworkAI(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"));
        Test(100, "FinalTests/HeuristicVSPolicyOnly.csv");

        Console.WriteLine("Testing batch nr. 11");
        Player1.PlayerAI = new HeuristicAI();
        Player2.PlayerAI = new MCTSnPolicy(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"));
        Test(100, "FinalTests/HeuristicVSMCTSnPolicy.csv");

        Console.WriteLine("Testing batch nr. 12");
        Player1.PlayerAI = new HeuristicAI();
        Player2.PlayerAI = new MCTSnCustomEval(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"));
        Test(100, "FinalTests/HeuristicVSMCTSnCustomEval.csv");

        Console.WriteLine("Testing batch nr. 13");
        Player1.PlayerAI = new HeuristicAI();
        Player2.PlayerAI = new MCTSnNNAI(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"), new SmallConvValueNetwork("Models/smallconvvaluemodel71.v0.nn"));
        Test(100, "FinalTests/HeuristicVSMCTSnNNAI.csv");

        Console.WriteLine("Testing batch nr. 14");
        Player1.PlayerAI = new MCTSnPolicy(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"));
        Player2.PlayerAI = new PolicyNetworkAI(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"));
        Test(100, "FinalTests/MCTSnPolicyVSPolicyOnly.csv");

        Console.WriteLine("Testing batch nr. 15");
        Player1.PlayerAI = new MCTSnPolicy(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"));
        Player2.PlayerAI = new MCTSnCustomEval(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"));
        Test(100, "FinalTests/MCTSnPolicyVSMCTSnCustomEval.csv");

        Console.WriteLine("Testing batch nr. 16");
        Player1.PlayerAI = new MCTSnCustomEval(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"));
        Player2.PlayerAI = new MinimaxAI();
        Test(100, "FinalTests/MCTSnCustomEvalVSMinimaxAI.csv");

        Console.WriteLine("Testing batch nr. 17");
        Player1.PlayerAI = new MCTSnCustomEval(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"));
        Player2.PlayerAI = new MCTSnNNAI(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"), new SmallConvValueNetwork("Models/smallconvvaluemodel71.v0.nn"));
        Test(100, "FinalTests/MCTSnCustomEvalVSMCTSnNNAI.csv");

        Console.WriteLine("Testing batch nr. 18");
        Player1.PlayerAI = new MCTSnCustomEval(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"));
        Player2.PlayerAI = new MCTSAIScoreDiffAvg();
        Test(100, "FinalTests/MCTSnCustomEvalVSMCTSAIScoreDiffAvg.csv");

        Console.WriteLine("Testing batch nr. 19");
        Player1.PlayerAI = new MCTSAIScoreDiffAvg();
        Player2.PlayerAI = new MinimaxAI();
        Test(100, "FinalTests/MCTSAIScoreDiffAvgVSMinimaxAI.csv");
    }

    public void RerunNetworkTests()
    {
        Console.WriteLine("Testing batch nr. 1");
        Player1.PlayerAI = new PolicyNetworkAI(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"));
        Player2.PlayerAI = new PolicyNetworkAI(new ConvolutionalPolicyNetwork("Models/fixedbigconvmodel10.v0.nn"));
        Test(100, "FinalTests/FixedSmallVSBigConvModel.csv");

        Console.WriteLine("Testing batch nr. 2");
        Player1.PlayerAI = new MCTSnPolicy(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"));
        Player2.PlayerAI = new MCTSnPolicy(new ConvolutionalPolicyNetwork("Models/fixedbigconvmodel10.v0.nn"));
        Test(100, "FinalTests/FixedSmallVSBigConvModelWithMCTS.csv");

        Console.WriteLine("Testing batch nr. 3");
        Player1.PlayerAI = new PolicyNetworkAI(new PolicyNetwork("Models/fixedbiglinearmodel12.v0.nn"));
        Player2.PlayerAI = new PolicyNetworkAI(new SmallLinearPolicyNetwork("Models/smalllinearmodel5.v0.nn"));
        Test(100, "FinalTests/FixedSmallVSBigLinearModel.csv");

        Console.WriteLine("Testing batch nr. 4");
        Player1.PlayerAI = new MCTSnPolicy(new PolicyNetwork("Models/fixedbiglinearmodel12.v0.nn"));
        Player2.PlayerAI = new MCTSnPolicy(new SmallLinearPolicyNetwork("Models/smalllinearmodel5.v0.nn"));
        Test(100, "FinalTests/FixedSmallVSBigLinearModelWithMCTS.csv");

        Console.WriteLine("Testing batch nr. 5");
        Player1.PlayerAI = new PolicyNetworkAI(new PolicyNetwork("Models/fixedbiglinearmodel12.v0.nn"));
        Player2.PlayerAI = new PolicyNetworkAI(new ConvolutionalPolicyNetwork("Models/fixedbigconvmodel10.v0.nn"));
        Test(100, "FinalTests/FixedConvVSLinearBigModel.csv");

        Console.WriteLine("Testing batch nr. 6");
        Player1.PlayerAI = new MCTSnPolicy(new PolicyNetwork("Models/fixedbiglinearmodel12.v0.nn"));
        Player2.PlayerAI = new MCTSnPolicy(new ConvolutionalPolicyNetwork("Models/fixedbigconvmodel10.v0.nn"));
        Test(100, "FinalTests/FixedConvVSLinearBigModelWithMCTS.csv");
    }

    public void RunAdditionalNetworkTests()
    {
        Console.WriteLine("Testing batch nr. 1");
        Player1.PlayerAI = new PolicyNetworkAI(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"));
        Player2.PlayerAI = new PolicyNetworkAI(new PolicyNetwork("Models/fixedbiglinearmodel12.v0.nn"));
        Test(100, "FinalTests/SmallConvVSBigLinearModel.csv");

        Console.WriteLine("Testing batch nr. 2");
        Player1.PlayerAI = new MCTSnPolicy(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"));
        Player2.PlayerAI = new MCTSnPolicy(new PolicyNetwork("Models/fixedbiglinearmodel12.v0.nn"));
        Test(100, "FinalTests/SmallConvVSBigLinearModelWithMCTS.csv");

        Console.WriteLine("Testing batch nr. 3");
        Player1.PlayerAI = new PolicyNetworkAI(new ConvolutionalPolicyNetwork("Models/fixedbigconvmodel10.v0.nn"));
        Player2.PlayerAI = new PolicyNetworkAI(new SmallLinearPolicyNetwork("Models/smalllinearmodel5.v0.nn"));
        Test(100, "FinalTests/BigConvVSSmallLinearModel.csv");

        Console.WriteLine("Testing batch nr. 4");
        Player1.PlayerAI = new MCTSnPolicy(new ConvolutionalPolicyNetwork("Models/fixedbigconvmodel10.v0.nn"));
        Player2.PlayerAI = new MCTSnPolicy(new SmallLinearPolicyNetwork("Models/smalllinearmodel5.v0.nn"));
        Test(100, "FinalTests/BigConvVSSmallLinearModelWithMCTS.csv");
    }

    public void RunTestsWithBigLinear()
    {
        Console.WriteLine("Testing batch nr. 1");
        Player1.PlayerAI = new HeuristicAI();
        Player2.PlayerAI = new PolicyNetworkAI(new PolicyNetwork("Models/fixedbiglinearmodel12.v0.nn"));
        Test(100, "FinalTests/BigLinearHeuristicVSPolicyOnly.csv");

        Console.WriteLine("Testing batch nr. 2");
        Player1.PlayerAI = new HeuristicAI();
        Player2.PlayerAI = new MCTSnPolicy(new PolicyNetwork("Models/fixedbiglinearmodel12.v0.nn"));
        Test(100, "FinalTests/BigLinearHeuristicVSMCTSnPolicy.csv");

        Console.WriteLine("Testing batch nr. 3");
        Player1.PlayerAI = new HeuristicAI();
        Player2.PlayerAI = new MCTSnCustomEval(new PolicyNetwork("Models/fixedbiglinearmodel12.v0.nn"));
        Test(100, "FinalTests/BigLinearHeuristicVSMCTSnCustomEval.csv");

        Console.WriteLine("Testing batch nr. 4");
        Player1.PlayerAI = new HeuristicAI();
        Player2.PlayerAI = new MCTSnNNAI(new PolicyNetwork("Models/fixedbiglinearmodel12.v0.nn"), new SmallConvValueNetwork("Models/smallconvvaluemodel71.v0.nn"));
        Test(100, "FinalTests/BigLinearHeuristicVSMCTSnNNAI.csv");

        Console.WriteLine("Testing batch nr. 5");
        Player1.PlayerAI = new MCTSnPolicy(new PolicyNetwork("Models/fixedbiglinearmodel12.v0.nn"));
        Player2.PlayerAI = new PolicyNetworkAI(new PolicyNetwork("Models/fixedbiglinearmodel12.v0.nn"));
        Test(100, "FinalTests/BigLinearMCTSnPolicyVSPolicyOnly.csv");

        Console.WriteLine("Testing batch nr. 6");
        Player1.PlayerAI = new MCTSnPolicy(new PolicyNetwork("Models/fixedbiglinearmodel12.v0.nn"));
        Player2.PlayerAI = new MCTSnCustomEval(new PolicyNetwork("Models/fixedbiglinearmodel12.v0.nn"));
        Test(100, "FinalTests/BigLinearMCTSnPolicyVSMCTSnCustomEval.csv");

        Console.WriteLine("Testing batch nr. 7");
        Player1.PlayerAI = new MCTSnCustomEval(new PolicyNetwork("Models/fixedbiglinearmodel12.v0.nn"));
        Player2.PlayerAI = new MinimaxAI();
        Test(100, "FinalTests/BigLinearMCTSnCustomEvalVSMinimaxAI.csv");

        Console.WriteLine("Testing batch nr. 8");
        Player1.PlayerAI = new MCTSnCustomEval(new PolicyNetwork("Models/fixedbiglinearmodel12.v0.nn"));
        Player2.PlayerAI = new MCTSnNNAI(new PolicyNetwork("Models/fixedbiglinearmodel12.v0.nn"), new SmallConvValueNetwork("Models/smallconvvaluemodel71.v0.nn"));
        Test(100, "FinalTests/BigLinearMCTSnCustomEvalVSMCTSnNNAI.csv");

        Console.WriteLine("Testing batch nr. 9");
        Player1.PlayerAI = new MCTSnCustomEval(new PolicyNetwork("Models/fixedbiglinearmodel12.v0.nn"));
        Player2.PlayerAI = new MCTSAIScoreDiffAvg();
        Test(100, "FinalTests/BigLinearMCTSnCustomEvalVSMCTSAIScoreDiffAvg.csv");
    }

    public void RunNetworkTestsWithMCTSandCustomValueFunction()
    {
        Console.WriteLine("Testing batch nr. 1");
        Player1.PlayerAI = new MCTSnCustomEval(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"));
        Player2.PlayerAI = new MCTSnCustomEval(new PolicyNetwork("Models/fixedbiglinearmodel12.v0.nn"));
        Test(100, "FinalTests/SmallConvVSBigLinearModelCustomEval.csv");

        Console.WriteLine("Testing batch nr. 2");
        Player1.PlayerAI = new MCTSnCustomEval(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"));
        Player2.PlayerAI = new MCTSnCustomEval(new ConvolutionalPolicyNetwork("Models/fixedbigconvmodel10.v0.nn"));
        Test(100, "FinalTests/SmallConvVSBigConvModelCustomEval.csv");

        Console.WriteLine("Testing batch nr. 3");
        Player1.PlayerAI = new MCTSnCustomEval(new SmallConvPolicyNetwork("Models/smallconvmodel9.v0.nn"));
        Player2.PlayerAI = new MCTSnCustomEval(new SmallLinearPolicyNetwork("Models/smalllinearmodel5.v0.nn"));
        Test(100, "FinalTests/SmallConvVSSmallLinearModelCustomEval.csv");

        Console.WriteLine("Testing batch nr. 4");
        Player1.PlayerAI = new MCTSnCustomEval(new PolicyNetwork("Models/fixedbiglinearmodel12.v0.nn"));
        Player2.PlayerAI = new MCTSnCustomEval(new ConvolutionalPolicyNetwork("Models/fixedbigconvmodel10.v0.nn"));
        Test(100, "FinalTests/BigLinearVSBigConvModelCustomEval.csv");

        Console.WriteLine("Testing batch nr. 5");
        Player1.PlayerAI = new MCTSnCustomEval(new PolicyNetwork("Models/fixedbiglinearmodel12.v0.nn"));
        Player2.PlayerAI = new MCTSnCustomEval(new SmallLinearPolicyNetwork("Models/smalllinearmodel5.v0.nn"));
        Test(100, "FinalTests/BigLinearVSSmallLinearModelCustomEval.csv");

        Console.WriteLine("Testing batch nr. 6");
        Player1.PlayerAI = new MCTSnCustomEval(new ConvolutionalPolicyNetwork("Models/fixedbigconvmodel10.v0.nn"));
        Player2.PlayerAI = new MCTSnCustomEval(new SmallLinearPolicyNetwork("Models/smalllinearmodel5.v0.nn"));
        Test(100, "FinalTests/BigConvVSSmallLinearModelCustomEval.csv");
    }

    public void RerunNetworkTestsWithNewModels()
    {
        Console.WriteLine("Testing batch nr. 1");
        Player1.PlayerAI = new PolicyNetworkAI(new SmallConvPolicyNetwork("Models/smallconvonhugemodel27.v2.nn"));
        Player2.PlayerAI = new PolicyNetworkAI(new ConvolutionalPolicyNetwork("Models/bigconvonhugemodel27.v0.nn"));
        Test(100, "FinalTests/NewModels/SmallVSBigConvModel.csv");

        Console.WriteLine("Testing batch nr. 2");
        Player1.PlayerAI = new MCTSnPolicy(new SmallConvPolicyNetwork("Models/smallconvonhugemodel27.v2.nn"));
        Player2.PlayerAI = new MCTSnPolicy(new ConvolutionalPolicyNetwork("Models/bigconvonhugemodel27.v0.nn"));
        Test(100, "FinalTests/NewModels/SmallVSBigConvModelWithMCTS.csv");

        Console.WriteLine("Testing batch nr. 3");
        Player1.PlayerAI = new PolicyNetworkAI(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Player2.PlayerAI = new PolicyNetworkAI(new PolicyNetwork("Models/biglinearonhugemodel16.v0.nn"));
        Test(100, "FinalTests/NewModels/SmallVSBigLinearModel.csv");

        Console.WriteLine("Testing batch nr. 4");
        Player1.PlayerAI = new MCTSnPolicy(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Player2.PlayerAI = new MCTSnPolicy(new PolicyNetwork("Models/biglinearonhugemodel16.v0.nn"));
        Test(100, "FinalTests/NewModels/SmallVSBigLinearModelWithMCTS.csv");

        Console.WriteLine("Testing batch nr. 5");
        Player1.PlayerAI = new PolicyNetworkAI(new ConvolutionalPolicyNetwork("Models/bigconvonhugemodel27.v0.nn"));
        Player2.PlayerAI = new PolicyNetworkAI(new PolicyNetwork("Models/biglinearonhugemodel16.v0.nn"));
        Test(100, "FinalTests/NewModels/ConvVSLinearBigModel.csv");

        Console.WriteLine("Testing batch nr. 6");
        Player1.PlayerAI = new MCTSnPolicy(new ConvolutionalPolicyNetwork("Models/bigconvonhugemodel27.v0.nn"));
        Player2.PlayerAI = new MCTSnPolicy(new PolicyNetwork("Models/biglinearonhugemodel16.v0.nn"));
        Test(100, "FinalTests/NewModels/ConvVSLinearBigModelWithMCTS.csv");

        Console.WriteLine("Testing batch nr. 7");
        Player1.PlayerAI = new PolicyNetworkAI(new SmallConvPolicyNetwork("Models/smallconvonhugemodel27.v2.nn"));
        Player2.PlayerAI = new PolicyNetworkAI(new PolicyNetwork("Models/biglinearonhugemodel16.v0.nn"));
        Test(100, "FinalTests/NewModels/SmallConvVSBigLinearModel.csv");

        Console.WriteLine("Testing batch nr. 8");
        Player1.PlayerAI = new MCTSnPolicy(new SmallConvPolicyNetwork("Models/smallconvonhugemodel27.v2.nn"));
        Player2.PlayerAI = new MCTSnPolicy(new PolicyNetwork("Models/biglinearonhugemodel16.v0.nn"));
        Test(100, "FinalTests/NewModels/SmallConvVSBigLinearModelWithMCTS.csv");

        Console.WriteLine("Testing batch nr. 9");
        Player1.PlayerAI = new PolicyNetworkAI(new ConvolutionalPolicyNetwork("Models/bigconvonhugemodel27.v0.nn"));
        Player2.PlayerAI = new PolicyNetworkAI(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Test(100, "FinalTests/NewModels/BigConvVSSmallLinearModel.csv");

        Console.WriteLine("Testing batch nr. 10");
        Player1.PlayerAI = new MCTSnPolicy(new ConvolutionalPolicyNetwork("Models/bigconvonhugemodel27.v0.nn"));
        Player2.PlayerAI = new MCTSnPolicy(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Test(100, "FinalTests/NewModels/BigConvVSSmallLinearModelWithMCTS.csv");

        Console.WriteLine("Testing batch nr. 11");
        Player1.PlayerAI = new PolicyNetworkAI(new SmallConvPolicyNetwork("Models/smallconvonhugemodel27.v2.nn"));
        Player2.PlayerAI = new PolicyNetworkAI(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Test(100, "FinalTests/NewModels/ConvVSLinearSmallModel.csv");

        Console.WriteLine("Testing batch nr. 12");
        Player1.PlayerAI = new MCTSnPolicy(new SmallConvPolicyNetwork("Models/smallconvonhugemodel27.v2.nn"));
        Player2.PlayerAI = new MCTSnPolicy(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Test(100, "FinalTests/NewModels/ConvVSLinearSmallModelWithMCTS.csv");
    }

    public void RunNewModelNetworkTestsWithMCTSandCustomValueFunction()
    {
        Console.WriteLine("Testing batch nr. 1");
        Player1.PlayerAI = new MCTSnCustomEval(new SmallConvPolicyNetwork("Models/smallconvonhugemodel27.v2.nn"));
        Player2.PlayerAI = new MCTSnCustomEval(new PolicyNetwork("Models/biglinearonhugemodel16.v0.nn"));
        Test(100, "FinalTests/NewModels/SmallConvVSBigLinearModelCustomEval.csv");

        Console.WriteLine("Testing batch nr. 2");
        Player1.PlayerAI = new MCTSnCustomEval(new SmallConvPolicyNetwork("Models/smallconvonhugemodel27.v2.nn"));
        Player2.PlayerAI = new MCTSnCustomEval(new ConvolutionalPolicyNetwork("Models/bigconvonhugemodel27.v0.nn"));
        Test(100, "FinalTests/NewModels/SmallConvVSBigConvModelCustomEval.csv");

        Console.WriteLine("Testing batch nr. 3");
        Player1.PlayerAI = new MCTSnCustomEval(new SmallConvPolicyNetwork("Models/smallconvonhugemodel27.v2.nn"));
        Player2.PlayerAI = new MCTSnCustomEval(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Test(100, "FinalTests/NewModels/SmallConvVSSmallLinearModelCustomEval.csv");

        Console.WriteLine("Testing batch nr. 4");
        Player1.PlayerAI = new MCTSnCustomEval(new PolicyNetwork("Models/biglinearonhugemodel16.v0.nn"));
        Player2.PlayerAI = new MCTSnCustomEval(new ConvolutionalPolicyNetwork("Models/bigconvonhugemodel27.v0.nn"));
        Test(100, "FinalTests/NewModels/BigLinearVSBigConvModelCustomEval.csv");

        Console.WriteLine("Testing batch nr. 5");
        Player1.PlayerAI = new MCTSnCustomEval(new PolicyNetwork("Models/biglinearonhugemodel16.v0.nn"));
        Player2.PlayerAI = new MCTSnCustomEval(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Test(100, "FinalTests/NewModels/BigLinearVSSmallLinearModelCustomEval.csv");

        Console.WriteLine("Testing batch nr. 6");
        Player1.PlayerAI = new MCTSnCustomEval(new ConvolutionalPolicyNetwork("Models/bigconvonhugemodel27.v0.nn"));
        Player2.PlayerAI = new MCTSnCustomEval(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Test(100, "FinalTests/NewModels/BigConvVSSmallLinearModelCustomEval.csv");
    }

    public void RunTestsWithNewSmallLinear()
    {
        // Heuristic

        Console.WriteLine("Testing batch nr. 1");
        Player1.PlayerAI = new HeuristicAI();
        Player2.PlayerAI = new PolicyNetworkAI(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Test(100, "FinalTests/NewModels/StrategyTests/HeuristicVSPolicyOnly.csv");

        Console.WriteLine("Testing batch nr. 2");
        Player1.PlayerAI = new HeuristicAI();
        Player2.PlayerAI = new MCTSnPolicy(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Test(100, "FinalTests/NewModels/StrategyTests/HeuristicVSMCTSnPolicy.csv");

        Console.WriteLine("Testing batch nr. 3");
        Player1.PlayerAI = new HeuristicAI();
        Player2.PlayerAI = new MCTSnCustomEval(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Test(100, "FinalTests/NewModels/StrategyTests/HeuristicVSMCTSnCustomEval.csv");

        Console.WriteLine("Testing batch nr. 4");
        Player1.PlayerAI = new HeuristicAI();
        Player2.PlayerAI = new MCTSnNNAI(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"), new SmallConvValueNetwork("Models/smallconvvaluemodel71.v0.nn"));
        Test(100, "FinalTests/NewModels/StrategyTests/HeuristicVSMCTSnNNAI.csv");

        // Classical

        Console.WriteLine("Testing batch nr. 5");
        Player1.PlayerAI = new MCTSnCustomEval(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Player2.PlayerAI = new MCTSAIScoreDiffAvg();
        Test(100, "FinalTests/NewModels/StrategyTests/MCTSnCustomEvalVSMCTSAIScoreDiffAvg.csv");

        Console.WriteLine("Testing batch nr. 6");
        Player1.PlayerAI = new MCTSnNNAI(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"), new SmallConvValueNetwork("Models/smallconvvaluemodel71.v0.nn"));
        Player2.PlayerAI = new MCTSAIScoreDiffAvg();
        Test(100, "FinalTests/NewModels/StrategyTests/MCTSnNNAIVSMCTSAIScoreDiffAvg.csv");

        Console.WriteLine("Testing batch nr. 7");
        Player1.PlayerAI = new MCTSnPolicy(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Player2.PlayerAI = new MCTSAIScoreDiffAvg();
        Test(100, "FinalTests/NewModels/StrategyTests/MCTSnPolicyVSMCTSAIScoreDiffAvg.csv");

        Console.WriteLine("Testing batch nr. 8");
        Player1.PlayerAI = new PolicyNetworkAI(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Player2.PlayerAI = new MCTSAIScoreDiffAvg();
        Test(100, "FinalTests/NewModels/StrategyTests/PolicyAIVSMCTSAIScoreDiffAvg.csv");

        // Minimax

        Console.WriteLine("Testing batch nr. 9");
        Player1.PlayerAI = new MCTSnCustomEval(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Player2.PlayerAI = new MinimaxAI();
        Test(100, "FinalTests/NewModels/StrategyTests/MCTSnCustomEvalVSMinimaxAI.csv");

        Console.WriteLine("Testing batch nr. 10");
        Player1.PlayerAI = new MCTSnNNAI(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"), new SmallConvValueNetwork("Models/smallconvvaluemodel71.v0.nn"));
        Player2.PlayerAI = new MinimaxAI();
        Test(100, "FinalTests/NewModels/StrategyTests/MCTSnNNAIVSMinimaxAI.csv");

        Console.WriteLine("Testing batch nr. 11");
        Player1.PlayerAI = new MCTSnPolicy(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Player2.PlayerAI = new MinimaxAI();
        Test(100, "FinalTests/NewModels/StrategyTests/MCTSnPolicyVSMinimaxAI.csv");

        Console.WriteLine("Testing batch nr. 12");
        Player1.PlayerAI = new PolicyNetworkAI(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Player2.PlayerAI = new MinimaxAI();
        Test(100, "FinalTests/NewModels/StrategyTests/PolicyAIVSMinimaxAI.csv");

        // Misc
        // Custom Eval

        Console.WriteLine("Testing batch nr. 13");
        Player1.PlayerAI = new MCTSnPolicy(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Player2.PlayerAI = new MCTSnCustomEval(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Test(100, "FinalTests/NewModels/StrategyTests/MCTSnPolicyVSMCTSnCustomEval.csv");

        Console.WriteLine("Testing batch nr. 14");
        Player1.PlayerAI = new MCTSnCustomEval(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Player2.PlayerAI = new MCTSnNNAI(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"), new SmallConvValueNetwork("Models/smallconvvaluemodel71.v0.nn"));
        Test(100, "FinalTests/NewModels/StrategyTests/MCTSnCustomEvalVSMCTSnNNAI.csv");

        Console.WriteLine("Testing batch nr. 14");
        Player1.PlayerAI = new MCTSnCustomEval(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Player2.PlayerAI = new PolicyNetworkAI(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Test(100, "FinalTests/NewModels/StrategyTests/MCTSnCustomEvalVSPolicyNetwork.csv");

        // MCTSnPolicy

        Console.WriteLine("Testing batch nr. 15");
        Player1.PlayerAI = new MCTSnPolicy(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Player2.PlayerAI = new PolicyNetworkAI(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Test(100, "FinalTests/NewModels/StrategyTests/MCTSnPolicyVSPolicyOnly.csv");

        Console.WriteLine("Testing batch nr. 16");
        Player1.PlayerAI = new MCTSnPolicy(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Player2.PlayerAI = new MCTSnNNAI(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"), new SmallConvValueNetwork("Models/smallconvvaluemodel71.v0.nn"));
        Test(100, "FinalTests/NewModels/StrategyTests/MCTSnPolicyVSMCTSnNNAI.csv");

        // Policy network vs MCTSnNNAI
        Console.WriteLine("Testing batch nr. 17");
        Player1.PlayerAI = new PolicyNetworkAI(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Player2.PlayerAI = new MCTSnNNAI(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"), new SmallConvValueNetwork("Models/smallconvvaluemodel71.v0.nn"));
        Test(100, "FinalTests/NewModels/StrategyTests/PolicyOnlyVSMCTSnNNAI.csv");
    }

    public void RunMCTSnNNAITestsWithNewValueNetwork()
    {
        Console.WriteLine("Testing batch nr. 1");
        Player1.PlayerAI = new HeuristicAI();
        Player2.PlayerAI = new MCTSnNNAI(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"), new SmallLinearValueNetwork("Models/smalllinearvalueonhugemodel48.v0.nn"));
        Test(100, "FinalTests/NewModels/NewValue/HeuristicVSMCTSnNNAI.csv");

        Console.WriteLine("Testing batch nr. 2");
        Player1.PlayerAI = new MCTSnNNAI(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"), new SmallLinearValueNetwork("Models/smalllinearvalueonhugemodel48.v0.nn"));
        Player2.PlayerAI = new MCTSAIScoreDiffAvg();
        Test(100, "FinalTests/NewModels/NewValue/MCTSnNNAIVSMCTSAIScoreDiffAvg.csv");

        Console.WriteLine("Testing batch nr. 3");
        Player1.PlayerAI = new MCTSnNNAI(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"), new SmallLinearValueNetwork("Models/smalllinearvalueonhugemodel48.v0.nn"));
        Player2.PlayerAI = new MinimaxAI();
        Test(100, "FinalTests/NewModels/NewValue/MCTSnNNAIVSMinimaxAI.csv");

        Console.WriteLine("Testing batch nr. 4");
        Player1.PlayerAI = new MCTSnCustomEval(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Player2.PlayerAI = new MCTSnNNAI(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"), new SmallLinearValueNetwork("Models/smalllinearvalueonhugemodel48.v0.nn"));
        Test(100, "FinalTests/NewModels/NewValue/MCTSnCustomEvalVSMCTSnNNAI.csv");

        Console.WriteLine("Testing batch nr. 5");
        Player1.PlayerAI = new MCTSnPolicy(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Player2.PlayerAI = new MCTSnNNAI(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"), new SmallLinearValueNetwork("Models/smalllinearvalueonhugemodel48.v0.nn"));
        Test(100, "FinalTests/NewModels/NewValue/MCTSnPolicyVSMCTSnNNAI.csv");

        Console.WriteLine("Testing batch nr. 6");
        Player1.PlayerAI = new PolicyNetworkAI(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"));
        Player2.PlayerAI = new MCTSnNNAI(new SmallLinearPolicyNetwork("Models/smalllinearonhugemodel15.v0.nn"), new SmallLinearValueNetwork("Models/smalllinearvalueonhugemodel48.v0.nn"));
        Test(100, "FinalTests/NewModels/NewValue/PolicyOnlyVSMCTSnNNAI.csv");
    }

    public void Test(int count, string filename) {
        for (int i = 0; i < count; i++) {
            Console.WriteLine("Playing game nr. " + i);
            int startingPlayer = i / ((count + players.Count - 1) / players.Count);
            gameState.NextRoundStartingPlayer = startingPlayer;
            gameState.CurrentPlayer = startingPlayer;
            PlayGame();
            WriteResults(filename, startingPlayer);
            //((MCTSnNNAI)Player2AI).SaveModel("Models/RL/RLPolicy.v1.nn", "Models/RL/RLValue.v1.nn");
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