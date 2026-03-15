using System.Linq;
using System.Threading.Tasks;
using Badminton_BE.Data;
using Badminton_BE.Models;
using Microsoft.EntityFrameworkCore;

namespace Badminton_BE.Services
{
    public class PlayerRankingService : IPlayerRankingService
    {
        private readonly AppDbContext _db;

        public PlayerRankingService(AppDbContext db)
        {
            _db = db;
        }

        public async Task SyncForMemberAsync(Member member)
        {
            var ranking = await GetRankingForMemberLevelAsync(member.Level);
            if (ranking == null)
            {
                return;
            }

            var playerRanking = await _db.RankingsByPlayer.FirstOrDefaultAsync(x => x.MemberId == member.Id);
            if (playerRanking == null)
            {
                playerRanking = new PlayerRanking
                {
                    MemberId = member.Id,
                    RankingId = ranking.Id,
                    EloPoint = ranking.DefaultEloPoint,
                    MatchesPlayed = 0,
                    Wins = 0,
                    Losses = 0,
                    Draws = 0
                };

                await _db.RankingsByPlayer.AddAsync(playerRanking);
            }
            else if (playerRanking.MatchesPlayed == 0 && playerRanking.Wins == 0 && playerRanking.Losses == 0 && playerRanking.Draws == 0)
            {
                playerRanking.RankingId = ranking.Id;
                playerRanking.EloPoint = ranking.DefaultEloPoint;
            }
            else
            {
                playerRanking.RankingId = ranking.Id;
            }

            await _db.SaveChangesAsync();
        }

        public async Task<int> BackfillMissingRankingsAsync()
        {
            var rankings = await _db.Rankings.AsNoTracking().ToListAsync();
            var membersWithoutRanking = await _db.Members
                .IgnoreQueryFilters()
                .Where(m => !_db.RankingsByPlayer.Any(pr => pr.MemberId == m.Id))
                .ToListAsync();

            var createdCount = 0;
            foreach (var member in membersWithoutRanking)
            {
                var ranking = MapMemberLevelToRanking(rankings, member.Level);
                if (ranking == null)
                {
                    continue;
                }

                await _db.RankingsByPlayer.AddAsync(new PlayerRanking
                {
                    MemberId = member.Id,
                    RankingId = ranking.Id,
                    EloPoint = ranking.DefaultEloPoint,
                    MatchesPlayed = 0,
                    Wins = 0,
                    Losses = 0,
                    Draws = 0
                });

                createdCount++;
            }

            if (createdCount > 0)
            {
                await _db.SaveChangesAsync();
            }

            return createdCount;
        }

        private async Task<Ranking?> GetRankingForMemberLevelAsync(MemberLevel level)
        {
            var rankings = await _db.Rankings.AsNoTracking().ToListAsync();
            return MapMemberLevelToRanking(rankings, level);
        }

        private static Ranking? MapMemberLevelToRanking(System.Collections.Generic.IEnumerable<Ranking> rankings, MemberLevel level)
        {
            var rankingName = level switch
            {
                MemberLevel.Newbie => "Newbie",
                MemberLevel.Beginner => "Yếu",
                MemberLevel.LowerIntermediate => "Trung bình yếu",
                MemberLevel.Intermediate => "Trung bình",
                MemberLevel.UpperIntermediate => "Trung bình khá",
                MemberLevel.Advance => "Khá",
                MemberLevel.Pro => "Giỏi",
                _ => "Newbie"
            };

            return rankings.FirstOrDefault(r => r.Name == rankingName);
        }
    }
}
