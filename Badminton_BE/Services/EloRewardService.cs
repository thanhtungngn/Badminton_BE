using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Badminton_BE.Models;
using Badminton_BE.Repositories;

namespace Badminton_BE.Services
{
    public class EloRewardService : IEloRewardService
    {
        private const int BaseReward = 25;
        private const int GapStep = 200;
        private const double BonusPerStep = 0.10;

        private readonly IPlayerRankingRepository _playerRankingRepo;
        private readonly IRankingRepository _rankingRepo;

        public EloRewardService(
            IPlayerRankingRepository playerRankingRepo,
            IRankingRepository rankingRepo)
        {
            _playerRankingRepo = playerRankingRepo;
            _rankingRepo = rankingRepo;
        }

        public async Task ApplyAsync(SessionMatch match)
        {
            if (match.Winner == MatchWinner.Pending || match.IsEloApplied)
            {
                return;
            }

            var teamA = match.Players.Where(p => p.Team == MatchTeam.TeamA).ToList();
            var teamB = match.Players.Where(p => p.Team == MatchTeam.TeamB).ToList();

            if (match.Winner == MatchWinner.Draw)
            {
                foreach (var p in match.Players)
                {
                    p.EloChange = 0;
                    var pr = await _playerRankingRepo.GetByMemberIdAsync(p.SessionPlayer!.MemberId);
                    if (pr == null) continue;
                    pr.MatchesPlayed++;
                    pr.Draws++;
                }
                match.IsEloApplied = true;
                return;
            }

            var reward = CalculateReward(teamA, teamB);
            var winners = match.Winner == MatchWinner.TeamA ? teamA : teamB;
            var losers = match.Winner == MatchWinner.TeamA ? teamB : teamA;

            var tiers = (await _rankingRepo.GetAllAsync())
                .OrderByDescending(r => r.DefaultEloPoint)
                .ToList();

            foreach (var p in winners)
            {
                p.EloChange = reward;
                var pr = await _playerRankingRepo.GetByMemberIdAsync(p.SessionPlayer!.MemberId);
                if (pr == null) continue;
                pr.EloPoint += reward;
                pr.Wins++;
                pr.MatchesPlayed++;
                SyncTier(pr, tiers);
            }

            foreach (var p in losers)
            {
                var pr = await _playerRankingRepo.GetByMemberIdAsync(p.SessionPlayer!.MemberId);
                if (pr == null) { p.EloChange = -reward; continue; }
                var actualLoss = Math.Min(pr.EloPoint, reward);
                p.EloChange = -actualLoss;
                pr.EloPoint -= actualLoss;
                pr.Losses++;
                pr.MatchesPlayed++;
                SyncTier(pr, tiers);
            }

            match.IsEloApplied = true;
        }

        public async Task ReverseAsync(SessionMatch match)
        {
            if (!match.IsEloApplied)
            {
                return;
            }

            var tiers = (await _rankingRepo.GetAllAsync())
                .OrderByDescending(r => r.DefaultEloPoint)
                .ToList();

            foreach (var p in match.Players)
            {
                var pr = await _playerRankingRepo.GetByMemberIdAsync(p.SessionPlayer!.MemberId);
                if (pr == null) continue;

                pr.EloPoint = Math.Max(0, pr.EloPoint - p.EloChange);
                pr.MatchesPlayed = Math.Max(0, pr.MatchesPlayed - 1);

                if (match.Winner == MatchWinner.Draw)
                {
                    pr.Draws = Math.Max(0, pr.Draws - 1);
                }
                else if (p.EloChange > 0)
                {
                    pr.Wins = Math.Max(0, pr.Wins - 1);
                }
                else if (p.EloChange < 0)
                {
                    pr.Losses = Math.Max(0, pr.Losses - 1);
                }

                SyncTier(pr, tiers);
            }

            match.IsEloApplied = false;
        }

        private static int CalculateReward(
            List<SessionMatchPlayer> teamA,
            List<SessionMatchPlayer> teamB)
        {
            var avgEloA = teamA.Average(p => (double)(p.SessionPlayer?.Member?.PlayerRanking?.EloPoint ?? 0));
            var avgEloB = teamB.Average(p => (double)(p.SessionPlayer?.Member?.PlayerRanking?.EloPoint ?? 0));
            var gap = Math.Abs(avgEloA - avgEloB);
            var steps = Math.Floor(gap / GapStep);
            return (int)Math.Round(BaseReward * (1 + steps * BonusPerStep));
        }

        private static void SyncTier(PlayerRanking playerRanking, List<Ranking> tiers)
        {
            var newTier = tiers.FirstOrDefault(r => r.DefaultEloPoint <= playerRanking.EloPoint);
            if (newTier != null)
            {
                playerRanking.RankingId = newTier.Id;
            }
        }
    }
}
