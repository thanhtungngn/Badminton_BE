using System.Collections.Generic;
using System.Threading.Tasks;
using Badminton_BE.Models;

namespace Badminton_BE.Repositories.Interfaces
{
    public interface IPlayerPaymentRepository : IRepository<PlayerPayment>
    {
        Task<PlayerPayment?> GetBySessionPlayerIdAsync(int sessionPlayerId);
        Task<IEnumerable<PlayerPayment>> GetBySessionIdAsync(int sessionId);
        Task<IEnumerable<PlayerPayment>> GetBySessionPlayerIdsAsync(IEnumerable<int> sessionPlayerIds);
    }
}
