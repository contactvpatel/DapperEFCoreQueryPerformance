using DapperEFCorePerformanceBenchmarks.DataAccess;
using DapperEFCorePerformanceBenchmarks.DTOs;
using DapperEFCorePerformanceBenchmarks.TestData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DapperEFCorePerformanceBenchmarks
{
    class Program
    {
        public static int NumPlayers { get; set; }
        public static int NumTeams { get; set; }
        public static int NumSports { get; set; }
        public static int NumRuns { get; set; }
        static void Main()
        {
            char input;
            do
            {
                ShowMenu();

                input = (Console.ReadLine() ?? string.Empty).First();
                switch (input)
                {
                    case 'Q':
                        break;

                    case 'T':
                        var testResults = new List<TestResult>();

                        Console.WriteLine("# of Test Runs:");
                        NumRuns = int.Parse(Console.ReadLine() ?? string.Empty);

                        //Gather Details for Test
                        Console.WriteLine("# of Sports per Run: ");
                        NumSports = int.Parse(Console.ReadLine() ?? string.Empty);

                        Console.WriteLine("# of Teams per Sport: ");
                        NumTeams = int.Parse(Console.ReadLine() ?? string.Empty);

                        Console.WriteLine("# of Players per Team: ");
                        NumPlayers = int.Parse(Console.ReadLine() ?? string.Empty);

                        #region Data Generation

                        var sports = Generator.GenerateSports(NumSports);
                        var teams = new List<TeamDTO>();
                        var players = new List<PlayerDTO>();
                        foreach (var sport in sports)
                        {
                            var newTeams = Generator.GenerateTeams(sport.Id, NumTeams);
                            teams.AddRange(newTeams);
                            foreach (var team in newTeams)
                            {
                                var newPlayers = Generator.GeneratePlayers(team.Id, NumPlayers);
                                players.AddRange(newPlayers);
                            }
                        }

                        Database.Reset();
                        Database.Load(sports, teams, players);

                        #endregion

                        #region Run Test

                        for (int i = 0; i < NumRuns + 1; i++)
                        {
                            if (i == 0)
                            {
                                //Discard first runs for warm up
                                EntityFrameworkCoreWithTracking firstTrackingTest = new EntityFrameworkCoreWithTracking();
                                RunTests(i, Framework.EntityFrameworkCoreWithTracking, firstTrackingTest);

                                EntityFrameworkNoTrackingCore firstEfNoTrackingCoreTest = new EntityFrameworkNoTrackingCore();
                                RunTests(i, Framework.EntityFrameworkCoreNoTracking, firstEfNoTrackingCoreTest);

                                DataAccess.Dapper firstDapperTest = new DataAccess.Dapper();
                                RunTests(i, Framework.Dapper, firstDapperTest);
                            }

                            //Run real tests
                            EntityFrameworkCoreWithTracking trackingTest = new EntityFrameworkCoreWithTracking();
                            testResults.AddRange(RunTests(i, Framework.EntityFrameworkCoreWithTracking, trackingTest));

                            EntityFrameworkNoTrackingCore efNoTrackingCoreTest = new EntityFrameworkNoTrackingCore();
                            testResults.AddRange(RunTests(i, Framework.EntityFrameworkCoreNoTracking, efNoTrackingCoreTest));

                            DataAccess.Dapper dapperTest = new DataAccess.Dapper();
                            testResults.AddRange(RunTests(i, Framework.Dapper, dapperTest));
                        }

                        ProcessResults(testResults);

                        #endregion

                        break;
                }

            } while (input != 'Q');
        }

        public static List<TestResult> RunTests(int runId, Framework framework, ITestSignature testSignature)
        {
            var results = new List<TestResult>();

            var result = new TestResult() { Run = runId, Framework = framework };
            double dataCount = 0;
            double timeCount = 0;
            for (var i = 1; i <= NumPlayers; i++)
            {
                var (item1, item2) = testSignature.GetPlayerByID(i);
                dataCount += item1;
                timeCount += item2;
            }
            result.PlayerByIdMilliseconds = Math.Round(timeCount/ NumPlayers, 3);
            result.PlayerDataCount = Math.Round(dataCount / NumPlayers, 3);

            dataCount = 0;
            timeCount = 0;

            for (var i = 1; i <= NumTeams; i++)
            {
                var (item1, item2) = testSignature.GetRosterByTeamID(i);
                dataCount += item1;
                timeCount += item2;
            }
            result.PlayersForTeamMilliseconds = Math.Round(timeCount / NumPlayers, 3);
            result.PlayersForTeamDataCount = Math.Round(dataCount / NumPlayers, 3);

            dataCount = 0;
            timeCount = 0;
            
            for (var i = 1; i <= NumSports; i++)
            {
                var (item1, item2) = testSignature.GetTeamRostersForSport(i);
                dataCount += item1;
                timeCount += item2;
            }
            result.TeamsForSportMilliseconds = Math.Round(timeCount / NumPlayers, 3);
            result.TeamsForSportDataCount = Math.Round(dataCount / NumPlayers, 3);
            
            results.Add(result);

            return results;
        }

        public static void ProcessResults(List<TestResult> results)
        {
            var groupedResults = results.GroupBy(x => x.Framework);
            foreach (var group in groupedResults)
            {
                Console.WriteLine(group.Key.ToString() + " Results - Microseconds");
                Console.WriteLine("Run #\tPlayer by ID\t\t\t\tPlayers per Team\t\t\t\tTeams per Sport");
                var orderedResults = group.OrderBy(x => x.Run);
                foreach (var orderResult in orderedResults)
                {
                    Console.WriteLine(orderResult.Run + "\t\t" +  orderResult.PlayerByIdMilliseconds + " μs / " + orderResult.PlayerDataCount + " (Data)\t\t" + orderResult.PlayersForTeamMilliseconds + " μs / " + orderResult.PlayersForTeamDataCount + " (Data)\t\t\t" + orderResult.TeamsForSportMilliseconds + " μs / " + orderResult.TeamsForSportDataCount + " (Data)");
                }
            }
        }

        public static void ShowMenu()
        {
            Console.WriteLine("Please enter one of the following options:");
            Console.WriteLine("Q - Quit");
            Console.WriteLine("T - Run Test");
            Console.WriteLine("Option:");
        }
    }
}