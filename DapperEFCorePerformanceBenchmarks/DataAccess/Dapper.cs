using System;
using Dapper;
using DapperEFCorePerformanceBenchmarks.DTOs;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace DapperEFCorePerformanceBenchmarks.DataAccess
{
    public class Dapper : ITestSignature
    {
        public Tuple<int, long> GetPlayerByID(int id)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            int dataCount;
            using (SqlConnection conn = new SqlConnection(Constants.SportsConnectionString))
            {
                conn.Open();
                var player = conn.QuerySingle<PlayerDTO>("SELECT Id, FirstName, LastName, DateOfBirth, TeamId FROM Player WHERE Id = @ID", new { ID = id });
                dataCount = player == null ? 0 : 1;
            }
            watch.Stop();
            return new Tuple<int, long>(dataCount, watch.ElapsedMilliseconds * 1000);
        }

        public Tuple<int, long> GetRosterByTeamID(int teamId)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            int dataCount;
            using (SqlConnection conn = new SqlConnection(Constants.SportsConnectionString))
            {
                conn.Open();

                var results = conn.QueryMultiple(@"
                                            SELECT Id, Name, SportID, FoundingDate FROM Team WHERE ID = @id
                                            SELECT Id, FirstName, LastName, DateOfBirth, TeamId FROM Player WHERE TeamId = @ID", new { id = teamId });

                var team = results.ReadSingle<TeamDTO>();
                var players = results.Read<PlayerDTO>();
                team.Players.AddRange(players);
                
                //var team = conn.QuerySingle<TeamDTO>("SELECT Id, Name, SportID, FoundingDate FROM Team WHERE ID = @id", new { id = teamId });
                //team.Players = conn.Query<PlayerDTO>("SELECT Id, FirstName, LastName, DateOfBirth, TeamId FROM Player WHERE TeamId = @ID", new { ID = teamId }).ToList();
                dataCount = team.Players.Count;
            }
            watch.Stop();
            return new Tuple<int, long>(dataCount, watch.ElapsedMilliseconds * 1000);
        }

        public Tuple<int, long> GetTeamRostersForSport(int sportId)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            int dataCount;
            using (SqlConnection conn = new SqlConnection(Constants.SportsConnectionString))
            {
                conn.Open();

                var teams = conn.Query<TeamDTO>("SELECT ID, Name, SportID, FoundingDate FROM Team WHERE SportID = @ID", new { ID = sportId });

                var teamIDs = teams.Select(x => x.Id).ToList();

                var players = conn.Query<PlayerDTO>("SELECT ID, FirstName, LastName, DateOfBirth, TeamID FROM Player WHERE TeamID IN @IDs", new { IDs = teamIDs });

                foreach (var team in teams)
                {
                    team.Players = players.Where(x => x.TeamId == team.Id).ToList();
                }
                dataCount = teams.Count();
            }
            watch.Stop();
            return new Tuple<int, long>(dataCount, watch.ElapsedMilliseconds * 1000);
        }
    }
}