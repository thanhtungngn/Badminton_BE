using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Badminton_BE.Data;
using Badminton_BE.Models;

namespace Badminton_BE.Repositories
{
    public class SessionPaymentRepository : Repository<SessionPayment>, ISessionPaymentRepository
    {
        public SessionPaymentRepository(AppDbContext db) : base(db) { }

        public async Task<SessionPayment?> GetBySessionIdAsync(int sessionId)
        {
            return await _db.Set<SessionPayment>()
                .FirstOrDefaultAsync(sp => sp.SessionId == sessionId);
        }
    }
}
