using System.Threading.Tasks;
using Badminton_BE.Models;

namespace Badminton_BE.Repositories.Interfaces
{
    public interface ISessionPaymentRepository : IRepository<SessionPayment>
    {
        Task<SessionPayment?> GetBySessionIdAsync(int sessionId);
    }
}
