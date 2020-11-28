using System;
using System.Collections.Generic;
using System.Text;

namespace DapperEFCorePerformanceBenchmarks.TestData
{
    public class TestResult
    {
        public double PlayerByIdMilliseconds { get; set; }
        public double PlayerDataCount { get; set; }
        public double PlayersForTeamMilliseconds { get; set; }
        public double PlayersForTeamDataCount { get; set; }
        public double TeamsForSportMilliseconds { get; set; }
        public double TeamsForSportDataCount { get; set; }
        public Framework Framework { get; set; }
        public int Run { get; set; }
    }

    public enum Framework
    {
        EntityFrameworkCoreNoTracking,
        EntityFrameworkCoreWithTracking,
        Dapper
    }
}
