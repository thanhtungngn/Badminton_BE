using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Badminton_BE.Data;
using Badminton_BE.Models;

namespace Badminton_BE.Repositories
{
    public class PlayerRankingRepository : Repository<PlayerRanking>, IPlayerRankingRepository
    {
        public PlayerRankingRepository(AppDbContext db) : base(db) { }

        public async Task<PlayerRanking?> GetByMemberIdAsync(int memberId)
        {
            return await _db.RankingsByPlayer
                .FirstOrDefaultAsync(pr => pr.MemberId == memberId);
        }

        public async Task<PlayerRanking?> GetByMemberIdWithRankingAsync(int memberId)
        {
            return await _db.RankingsByPlayer
                .AsNoTracking()
                .Include(pr => pr.Ranking)
                .FirstOrDefaultAsync(pr => pr.MemberId == memberId);
        }
    }
}
