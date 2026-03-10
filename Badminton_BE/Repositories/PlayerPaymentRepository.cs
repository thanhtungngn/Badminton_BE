using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Badminton_BE.Data;
using Badminton_BE.Models;

namespace Badminton_BE.Repositories
{
    public class PlayerPaymentRepository : Repository<PlayerPayment>, IPlayerPaymentRepository
    {
        public PlayerPaymentRepository(AppDbContext db) : base(db) { }

        public async Task<PlayerPayment?> GetBySessionPlayerIdAsync(int sessionPlayerId)
        {
            return await _db.Set<PlayerPayment>()
                .FirstOrDefaultAsync(p => p.SessionPlayerId == sessionPlayerId);
        }

        public async Task<IEnumerable<PlayerPayment>> GetBySessionIdAsync(int sessionId)
        {
            // join SessionPayment -> Session -> SessionPlayers -> PlayerPayment
            return await _db.PlayerPayments
                .Include(p => p.SessionPlayer)
                .Where(p => p.SessionPlayer != null && p.SessionPlayer.SessionId == sessionId)
                .ToListAsync();
        }
    }
}
