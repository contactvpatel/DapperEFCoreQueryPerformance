using System;
using System.Collections.Generic;
using System.Text;

namespace DapperEFCorePerformanceBenchmarks.DataAccess
{
    public interface ITestSignature
    {
        Tuple<int, long> GetPlayerByID(int id);
        Tuple<int, long> GetRosterByTeamID(int teamID);
        Tuple<int, long> GetTeamRostersForSport(int sportID);
    }
}
