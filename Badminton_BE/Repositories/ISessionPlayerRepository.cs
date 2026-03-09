using System.Threading.Tasks;
using Badminton_BE.Models;

namespace Badminton_BE.Repositories
{
    public interface ISessionPlayerRepository : IRepository<SessionPlayer>
    {
        Task<SessionPlayer?> GetBySessionAndMemberAsync(int sessionId, int memberId);
        Task<SessionPlayer?> GetByIdWithIncludesAsync(int id);
        Task<bool> HasOverlappingSessionAsync(int memberId, DateTime start, DateTime end);
    }
}
