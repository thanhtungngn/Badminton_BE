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
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Member?> GetByIdWithContactsAsync(int id)
        {
            return await _db.Members
                .Include(m => m.Contacts)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Member?> GetByContactValueAsync(string contactValue)
        {
            // find the member that has a contact with the given value
            return await _db.Members
                .Include(m => m.Contacts)
                .Where(m => m.Contacts.Any(c => c.ContactValue == contactValue))
                .FirstOrDefaultAsync();
        }
    }
}
