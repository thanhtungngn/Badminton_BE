using System.Collections.Generic;
using System.Threading.Tasks;
using Badminton_BE.Models;

namespace Badminton_BE.Repositories.Interfaces
{
    public interface ISessionMatchRepository : IRepository<SessionMatch>
    {
        Task<IEnumerable<SessionMatch>> GetBySessionIdAsync(int sessionId);
        Task<SessionMatch?> GetByIdWithPlayersAsync(int id);
        Task<SessionMatch?> GetBySessionAndMatchIdAsync(int sessionId, int matchId);
    }
}
