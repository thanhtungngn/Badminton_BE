using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Badminton_BE.Data;
using Badminton_BE.Models;

namespace Badminton_BE.Repositories
{
    public class SessionRepository : Repository<Session>, ISessionRepository
    {
        public SessionRepository(AppDbContext db) : base(db) { }

        public async Task<IEnumerable<Session>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _db.Sessions
                .AsNoTracking()
                .Where(s => s.StartTime >= start && s.EndTime <= end)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<Session?> GetByIdWithPlayersAsync(int id)
        {
            return await _db.Sessions
                .Include(s => s.SessionPlayers!)
                    .ThenInclude(sp => sp.Member)
                        .ThenInclude(m => m.Contacts)
                .Include(s => s.SessionPlayers!)
                    .ThenInclude(sp => sp.Member)
                        .ThenInclude(m => m.PlayerRanking)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}
