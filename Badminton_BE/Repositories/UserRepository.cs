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
    }
}
