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

            var normalizedSearch = contactValue.Trim().Replace(" ", "");
            var altSearch = normalizedSearch.StartsWith("+84") 
                ? "0" + normalizedSearch.Substring(3) 
                : (normalizedSearch.StartsWith("0") ? "+84" + normalizedSearch.Substring(1) : normalizedSearch);

            return await _db.Members
                .Include(m => m.Contacts)
                .Where(m => m.Contacts.Any(c => 
                    c.ContactValue == normalizedSearch || 
                    c.ContactValue == altSearch ||
                    c.ContactValue.Trim() == normalizedSearch ||
                    c.ContactValue.Trim() == altSearch))
                .FirstOrDefaultAsync();
        }

        public async Task<Member?> GetByContactValueIgnoreFiltersAsync(string contactValue)
        {
            if (string.IsNullOrWhiteSpace(contactValue))
            {
                return null;
            }

            var normalizedSearch = contactValue.Trim().Replace(" ", "");
            var altSearch = normalizedSearch.StartsWith("+84") 
                ? "0" + normalizedSearch.Substring(3) 
                : (normalizedSearch.StartsWith("0") ? "+84" + normalizedSearch.Substring(1) : normalizedSearch);

            var memberId = await _db.Contacts
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(c => 
                    c.ContactValue == normalizedSearch || 
                    c.ContactValue == altSearch ||
                    c.ContactValue.Trim() == normalizedSearch ||
                    c.ContactValue.Trim() == altSearch)
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
                .FirstOrDefaultAsync(m => m.Id == memberId.Value);
        }

        public async Task<IEnumerable<Member>> GetAllByContactValueIgnoreFiltersAsync(string contactValue)
        {
            if (string.IsNullOrWhiteSpace(contactValue))
            {
                return Enumerable.Empty<Member>();
            }

            var normalizedSearch = contactValue.Trim().Replace(" ", "");
            var altSearch = normalizedSearch.StartsWith("+84") 
                ? "0" + normalizedSearch.Substring(3) 
                : (normalizedSearch.StartsWith("0") ? "+84" + normalizedSearch.Substring(1) : normalizedSearch);

            var memberIds = await _db.Contacts
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(c => 
                    c.ContactValue == normalizedSearch || 
                    c.ContactValue == altSearch ||
                    c.ContactValue.Trim() == normalizedSearch ||
                    c.ContactValue.Trim() == altSearch)
                .Select(c => c.MemberId)
                .Distinct()
                .ToListAsync();

            if (!memberIds.Any())
            {
                return Enumerable.Empty<Member>();
            }

            return await _db.Members
                .IgnoreQueryFilters()
                .Include(m => m.Contacts)
                .Include(m => m.PlayerRanking)
                    .ThenInclude(pr => pr.Ranking)
                .Where(m => memberIds.Contains(m.Id))
                .ToListAsync();
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
                .Where(c => c.UserId == userId
                    && c.ContactType == ContactType.Phone
                    && (c.ContactValue == normalizedPhoneNumber || c.ContactValue.Trim() == normalizedPhoneNumber))
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
                .Include(m => m.PlayerRanking)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == memberId.Value && m.UserId == userId);
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
