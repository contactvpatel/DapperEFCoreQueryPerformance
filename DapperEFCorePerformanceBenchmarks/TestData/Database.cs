using DapperEFCorePerformanceBenchmarks.DTOs;
using DapperEFCorePerformanceBenchmarks.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DapperEFCorePerformanceBenchmarks.TestData
{
    public static class Database
    {
        public static void Reset()
        {
            using (SportContextEfCore context = new SportContextEfCore(GetOptions()))
            {
                context.Database.ExecuteSqlRaw("DELETE FROM Player");
                context.Database.ExecuteSqlRaw("DELETE FROM Team");
                context.Database.ExecuteSqlRaw("DELETE FROM Sport");
            }
        }

        public static void Load(List<SportDTO> sports, List<TeamDTO> teams, List<PlayerDTO> players)
        {
            AddSports(sports);
            AddTeams(teams);
            AddPlayers(players);
        }

        private static void AddPlayers(List<PlayerDTO> players)
        {
            using SportContextEfCore context = new SportContextEfCore(GetOptions());
            using var transaction = context.Database.BeginTransaction();
            context.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT [dbo].[Player] ON");
            foreach (var player in players)
            {
                context.Players.Add(new Player()
                {
                    FirstName = player.FirstName,
                    LastName = player.LastName,
                    DateOfBirth = player.DateOfBirth,
                    TeamId = player.TeamId,
                    Id = player.Id
                });
            }

            context.SaveChanges();
            context.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT [dbo].[Player] OFF");
            transaction.Commit();
        }

        private static void AddTeams(List<TeamDTO> teams)
        {
            using SportContextEfCore context = new SportContextEfCore(GetOptions());
            using var transaction = context.Database.BeginTransaction();
            context.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT [dbo].[Team] ON");
            foreach (var team in teams)
            {
                context.Teams.Add(new Team()
                {
                    Name = team.Name,
                    Id = team.Id,
                    SportId = team.SportId,
                    FoundingDate = team.FoundingDate
                });
            }
            context.SaveChanges();
            context.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT [dbo].[Team] OFF");
            transaction.Commit();
        }

        private static void AddSports(List<SportDTO> sports)
        {
            using SportContextEfCore context = new SportContextEfCore(GetOptions());
            using var transaction = context.Database.BeginTransaction();
            context.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT [dbo].[Sport] ON");
            foreach (var sport in sports)
            {
                context.Sports.Add(new Sport()
                {
                    Id = sport.Id,
                    Name = sport.Name
                });
            }
            context.SaveChanges();
            context.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT [dbo].[Sport] OFF");
            transaction.Commit();
        }

        public static DbContextOptions GetOptions()
        {
            DbContextOptionsBuilder builder = new DbContextOptionsBuilder();
            builder.UseSqlServer(Constants.SportsConnectionString);

            return builder.Options;
        }
    }
}
