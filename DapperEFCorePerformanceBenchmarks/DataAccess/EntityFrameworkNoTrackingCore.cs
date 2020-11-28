using System;
using DapperEFCorePerformanceBenchmarks.Models;
using DapperEFCorePerformanceBenchmarks.TestData;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;

namespace DapperEFCorePerformanceBenchmarks.DataAccess
{
    public class EntityFrameworkNoTrackingCore : ITestSignature
    {
        public Tuple<int, long> GetPlayerByID(int id)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            int dataCount;
            using (SportContextEfCore context = new SportContextEfCore(Database.GetOptions()))
            {
                var player = context.Players.First(x => x.Id == id);
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
            using (SportContextEfCore context = new SportContextEfCore(Database.GetOptions()))
            {
                var team = context.Teams.Include(x => x.Players).AsNoTracking().Single(x => x.Id == teamId);
                dataCount = team?.Players.Count ?? 0;
            }
            watch.Stop();
            return new Tuple<int, long>(dataCount, watch.ElapsedMilliseconds * 1000);
        }

        public Tuple<int, long> GetTeamRostersForSport(int sportId)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            int dataCount;
            using (SportContextEfCore context = new SportContextEfCore(Database.GetOptions()))
            {
                var teams = context.Teams.Include(x => x.Players).Where(x => x.SportId == sportId).AsNoTracking().ToList();
                dataCount = teams.Count;
            }
            watch.Stop();
            return new Tuple<int, long>(dataCount, watch.ElapsedMilliseconds * 1000);
        }
    }
}
