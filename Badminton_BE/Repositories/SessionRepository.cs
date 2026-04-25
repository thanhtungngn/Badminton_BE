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
                .Include(s => s.Matches!)
                    .ThenInclude(m => m.Players)
                        .ThenInclude(mp => mp.SessionPlayer)
                            .ThenInclude(sp => sp.Member)
                                .ThenInclude(m => m.PlayerRanking)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Session>> GetByParticipantPhoneNumberAsync(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return Enumerable.Empty<Session>();

            var normalizedSearch = phoneNumber.Trim().Replace(" ", "");
            var altSearch = normalizedSearch.StartsWith("+84") 
                ? "0" + normalizedSearch.Substring(3) 
                : (normalizedSearch.StartsWith("0") ? "+84" + normalizedSearch.Substring(1) : normalizedSearch);

            // Step 1: Find all member IDs matching this phone, bypassing all query filters
            var memberIds = await _db.Contacts
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(c => c.ContactType == ContactType.Phone &&
                    (c.ContactValue == normalizedSearch || c.ContactValue == altSearch ||
                     c.ContactValue.Trim() == normalizedSearch || c.ContactValue.Trim() == altSearch))
                .Select(c => c.MemberId)
                .Distinct()
                .ToListAsync();

            if (!memberIds.Any()) return Enumerable.Empty<Session>();

            // Step 2: Find all session IDs where these members participated
            var sessionIds = await _db.Set<SessionPlayer>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(sp => memberIds.Contains(sp.MemberId))
                .Select(sp => sp.SessionId)
                .Distinct()
                .ToListAsync();

            if (!sessionIds.Any()) return Enumerable.Empty<Session>();

            // Step 3: Load those sessions (bypassing the host-user filter)
            return await _db.Sessions
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(s => s.SessionPlayers)
                .Where(s => sessionIds.Contains(s.Id))
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();
        }
    }
}
