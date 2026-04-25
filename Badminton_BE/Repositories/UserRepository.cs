using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Badminton_BE.Data;
using Badminton_BE.Models;

namespace Badminton_BE.Repositories
{
    public class UserRepository : Repository<AppUser>, IUserRepository
    {
        public UserRepository(AppDbContext db) : base(db) { }

        public Task<AppUser?> GetByNormalizedUsernameAsync(string normalizedUsername)
        {
            return _db.Set<AppUser>().FirstOrDefaultAsync(u => u.NormalizedUsername == normalizedUsername);
        }

        public Task<AppUser?> GetByPhoneNumberAsync(string phoneNumber)
        {
            var normalizedSearch = phoneNumber.Trim().Replace(" ", "");
            var altSearch = normalizedSearch.StartsWith("+84") 
                ? "0" + normalizedSearch.Substring(3) 
                : (normalizedSearch.StartsWith("0") ? "+84" + normalizedSearch.Substring(1) : normalizedSearch);

            return _db.Set<AppUser>().FirstOrDefaultAsync(u => 
                u.PhoneNumber == normalizedSearch || 
                u.PhoneNumber == altSearch ||
                u.PhoneNumber.Trim() == normalizedSearch ||
                u.PhoneNumber.Trim() == altSearch);
        }

        public Task<AppUser?> GetByEmailAsync(string email)
        {
            var normalized = email.Trim().ToLowerInvariant();
            return _db.Set<AppUser>().FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == normalized);
        }
    }
}
