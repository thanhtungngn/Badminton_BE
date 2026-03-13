using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Badminton_BE.Data;
using Badminton_BE.Models;

namespace Badminton_BE.Repositories
{
    public class RevokedTokenRepository : Repository<RevokedToken>, IRevokedTokenRepository
    {
        public RevokedTokenRepository(AppDbContext db) : base(db) { }

        public Task<RevokedToken?> GetByJtiAsync(string jti)
        {
            return _db.Set<RevokedToken>().IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Jti == jti);
        }

        public Task<bool> IsRevokedAsync(string jti)
        {
            return _db.Set<RevokedToken>().IgnoreQueryFilters().AnyAsync(x => x.Jti == jti);
        }
    }
}
