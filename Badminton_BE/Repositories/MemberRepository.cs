using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Badminton_BE.Data;
using Badminton_BE.Models;
using System.Linq;

namespace Badminton_BE.Repositories
{
    public class MemberRepository : Repository<Member>, IMemberRepository
    {
        public MemberRepository(AppDbContext db) : base(db) { }

        public async Task<IEnumerable<Member>> GetAllWithContactsAsync()
        {
            return await _db.Members
                .Include(m => m.Contacts)
                .Include(m => m.PlayerRanking)
                    .ThenInclude(pr => pr.Ranking)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Get all members who have participated in at least one session owned by the given host.
        /// </summary>
        public async Task<IEnumerable<Member>> GetAllForHostAsync(int hostUserId)
        {
            // Find all member IDs that have a SessionPlayer entry in a session owned by this host
            var memberIds = await _db.SessionPlayers
                .IgnoreQueryFilters()
                .Where(sp => sp.UserId == hostUserId)
                .Select(sp => sp.MemberId)
                .Distinct()
                .ToListAsync();

            return await _db.Members
                .Include(m => m.Contacts)
                .Include(m => m.PlayerRanking)
                    .ThenInclude(pr => pr.Ranking)
                .Where(m => memberIds.Contains(m.Id))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Member?> GetByIdWithContactsAsync(int id)
        {
            return await _db.Members
                .Include(m => m.Contacts)
                .Include(m => m.PlayerRanking)
                    .ThenInclude(pr => pr.Ranking)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Member?> GetByContactValueAsync(string contactValue)
        {
            if (string.IsNullOrWhiteSpace(contactValue))
            {
                return null;
            }

            var normalizedContactValue = contactValue.Trim();

            return await _db.Members
                .Include(m => m.Contacts)
                .Where(m => m.Contacts.Any(c => c.ContactValue == normalizedContactValue || c.ContactValue.Trim() == normalizedContactValue))
                .FirstOrDefaultAsync();
        }

        public async Task<Member?> GetByContactValueIgnoreFiltersAsync(string contactValue)
        {
            if (string.IsNullOrWhiteSpace(contactValue))
            {
                return null;
            }

            var normalizedContactValue = contactValue.Trim();

            var memberId = await _db.Contacts
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(c => c.ContactValue == normalizedContactValue || c.ContactValue.Trim() == normalizedContactValue)
                .OrderByDescending(c => c.IsPrimary)
                .Select(c => (int?)c.MemberId)
                .FirstOrDefaultAsync();

            if (!memberId.HasValue)
            {
                return null;
            }

            return await _db.Members
                .IgnoreQueryFilters()
                .Include(m => m.Contacts)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == memberId.Value);
        }

        public async Task<Member?> GetByPhoneNumberForUserIgnoreFiltersAsync(int userId, string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return null;
            }

            var normalizedPhoneNumber = phoneNumber.Trim();

            var memberId = await _db.Contacts
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(c => c.ContactType == ContactType.Phone
                    && (c.ContactValue == normalizedPhoneNumber || c.ContactValue.Trim() == normalizedPhoneNumber))
                .OrderByDescending(c => c.IsPrimary)
                .Select(c => (int?)c.MemberId)
                .FirstOrDefaultAsync();

            if (!memberId.HasValue)
            {
                return null;
            }

            return await _db.Members
                .Include(m => m.Contacts)
                .Include(m => m.PlayerRanking)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == memberId.Value);
        }

        public async Task<IEnumerable<Member>> GetMembersWithoutPlayerRankingAsync()
        {
            return await _db.Members
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(m => !_db.RankingsByPlayer.Any(pr => pr.MemberId == m.Id))
                .ToListAsync();
        }
    }
}
