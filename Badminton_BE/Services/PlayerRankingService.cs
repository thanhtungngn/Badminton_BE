using System.Linq;
using System.Threading.Tasks;
using Badminton_BE.Models;

namespace Badminton_BE.Services
{
    public class PlayerRankingService : IPlayerRankingService
    {
        private readonly IMemberRepository _memberRepository;
        private readonly IRankingRepository _rankingRepository;
        private readonly IPlayerRankingRepository _playerRankingRepository;

        public PlayerRankingService(
            IMemberRepository memberRepository,
            IRankingRepository rankingRepository,
            IPlayerRankingRepository playerRankingRepository)
        {
            _memberRepository = memberRepository;
            _rankingRepository = rankingRepository;
            _playerRankingRepository = playerRankingRepository;
        }

        public async Task SyncForMemberAsync(Member member)
        {
            var ranking = await GetRankingForMemberLevelAsync(member.Level);
            if (ranking == null)
            {
                return;
            }

            var playerRanking = await _playerRankingRepository.GetByMemberIdAsync(member.Id);
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

                await _playerRankingRepository.AddAsync(playerRanking);
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

            await _playerRankingRepository.SaveChangesAsync();
        }

        public async Task<int> BackfillMissingRankingsAsync()
        {
            var rankings = await _rankingRepository.GetAllAsync();
            var membersWithoutRanking = await _memberRepository.GetMembersWithoutPlayerRankingAsync();

            var createdCount = 0;
            foreach (var member in membersWithoutRanking)
            {
                var ranking = MapMemberLevelToRanking(rankings, member.Level);
                if (ranking == null)
                {
                    continue;
                }

                await _playerRankingRepository.AddAsync(new PlayerRanking
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
                await _playerRankingRepository.SaveChangesAsync();
            }

            return createdCount;
        }

        private async Task<Ranking?> GetRankingForMemberLevelAsync(MemberLevel level)
        {
            var rankings = await _rankingRepository.GetAllAsync();
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
