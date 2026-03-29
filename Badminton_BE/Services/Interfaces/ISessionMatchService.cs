using System.Collections.Generic;
using System.Threading.Tasks;
using Badminton_BE.DTOs;

namespace Badminton_BE.Services.Interfaces
{
    public interface ISessionMatchService
    {
        Task<IEnumerable<SessionMatchReadDto>> GetBySessionIdAsync(int sessionId);
        Task<SessionMatchReadDto?> GetByIdAsync(int sessionId, int matchId);
        Task<SessionMatchReadDto?> CreateAsync(int sessionId, SessionMatchUpsertDto dto);
        Task<SessionMatchReadDto?> UpdateAsync(int sessionId, int matchId, SessionMatchUpsertDto dto);
        Task<bool> DeleteAsync(int sessionId, int matchId);
    }
}
