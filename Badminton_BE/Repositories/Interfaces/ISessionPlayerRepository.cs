using System.Threading.Tasks;
using System.Collections.Generic;
using Badminton_BE.Models;

namespace Badminton_BE.Repositories.Interfaces
{
    public interface ISessionPlayerRepository : IRepository<SessionPlayer>
    {
        Task<SessionPlayer?> GetBySessionAndMemberAsync(int sessionId, int memberId);
        Task<SessionPlayer?> GetByIdWithIncludesAsync(int id);
        Task<IEnumerable<SessionPlayer>> GetByMemberIdWithSessionAsync(int memberId);
        Task<bool> HasOverlappingSessionAsync(int memberId, DateTime start, DateTime end);
        Task<int> CountActiveBySessionAsync(int sessionId);
    }
}
