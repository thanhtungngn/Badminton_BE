using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Badminton_BE.Data;
using Badminton_BE.Models;

namespace Badminton_BE.Repositories
{
    public class SessionMatchRepository : Repository<SessionMatch>, ISessionMatchRepository
    {
        public SessionMatchRepository(AppDbContext db) : base(db) { }

        public async Task<IEnumerable<SessionMatch>> GetBySessionIdAsync(int sessionId)
        {
            return await _db.SessionMatches
                .Include(m => m.Players)
                    .ThenInclude(p => p.SessionPlayer)
                        .ThenInclude(sp => sp.Member)
                            .ThenInclude(m => m.PlayerRanking)
                .AsNoTracking()
                .Where(m => m.SessionId == sessionId)
                .OrderByDescending(m => m.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<SessionMatch>> GetByMemberIdAsync(int memberId, int? ownerUserId = null)
        {
            var query = _db.SessionMatches
                .Include(m => m.Players)
                    .ThenInclude(p => p.SessionPlayer)
                        .ThenInclude(sp => sp.Member)
                            .ThenInclude(m => m.PlayerRanking)
                .AsNoTracking()
                .Where(m => m.Players.Any(p => p.SessionPlayer != null && p.SessionPlayer.MemberId == memberId));

            if (ownerUserId.HasValue)
            {
                query = query.Where(m => m.UserId == ownerUserId.Value);
            }

            return await query
                .OrderByDescending(m => m.CreatedDate)
                .ToListAsync();
        }

        public async Task<SessionMatch?> GetByIdWithPlayersAsync(int id)
        {
            return await _db.SessionMatches
                .Include(m => m.Players)
                    .ThenInclude(p => p.SessionPlayer)
                        .ThenInclude(sp => sp.Member)
                            .ThenInclude(m => m.PlayerRanking)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<SessionMatch?> GetBySessionAndMatchIdAsync(int sessionId, int matchId)
        {
            return await _db.SessionMatches
                .Include(m => m.Players)
                    .ThenInclude(p => p.SessionPlayer)
                        .ThenInclude(sp => sp.Member)
                            .ThenInclude(m => m.PlayerRanking)
                .FirstOrDefaultAsync(m => m.SessionId == sessionId && m.Id == matchId);
        }
    }
}
