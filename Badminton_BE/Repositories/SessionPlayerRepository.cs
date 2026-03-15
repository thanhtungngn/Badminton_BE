using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Badminton_BE.Data;
using Badminton_BE.Models;

namespace Badminton_BE.Repositories
{
    public class SessionPlayerRepository : Repository<SessionPlayer>, ISessionPlayerRepository
    {
        public SessionPlayerRepository(AppDbContext db) : base(db) { }

        public async Task<SessionPlayer?> GetBySessionAndMemberAsync(int sessionId, int memberId)
        {
            return await _db.Set<SessionPlayer>()
                .Include(sp => sp.Member)
                .Include(sp => sp.Session)
                .FirstOrDefaultAsync(sp => sp.SessionId == sessionId && sp.MemberId == memberId);
        }

        public async Task<SessionPlayer?> GetByIdWithIncludesAsync(int id)
        {
            return await _db.Set<SessionPlayer>()
                .Include(sp => sp.Member)
                .Include(sp => sp.Session)
                .FirstOrDefaultAsync(sp => sp.Id == id);
        }

        public async Task<IEnumerable<SessionPlayer>> GetByMemberIdWithSessionAsync(int memberId)
        {
            return await _db.Set<SessionPlayer>()
                .IgnoreQueryFilters()
                .Include(sp => sp.Session)
                .AsNoTracking()
                .Where(sp => sp.MemberId == memberId)
                .OrderByDescending(sp => sp.Session != null ? sp.Session.StartTime : DateTime.MinValue)
                .ToListAsync();
        }

        public async Task<bool> HasOverlappingSessionAsync(int memberId, DateTime start, DateTime end)
        {
            return await _db.Set<SessionPlayer>()
                .Include(sp => sp.Session)
                .Where(sp => sp.MemberId == memberId)
                .AnyAsync(sp =>
                    // session must overlap with given time window
                    sp.Session != null &&
                    sp.Session.StartTime < end &&
                    sp.Session.EndTime > start &&
                    // only consider upcoming or ongoing sessions
                    (sp.Session.Status == SessionStatus.Upcoming || sp.Session.Status == SessionStatus.OnGoing)
                );
        }
    }
}
