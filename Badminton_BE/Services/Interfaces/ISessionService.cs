using System.Collections.Generic;
using System.Threading.Tasks;
using Badminton_BE.DTOs;

namespace Badminton_BE.Services.Interfaces
{
    public interface ISessionService
    {
        Task<SessionReadDto> CreateSessionAsync(SessionCreateDto dto);
        Task<IEnumerable<SessionReadDto>> GetSessionsAsync();
        Task<IEnumerable<SessionReadDto>> GetActiveSessionsAsync();
        Task<SessionReadDto?> GetSessionByIdAsync(int id);
        Task<SessionWithPlayersDto?> GetSessionDetailAsync(int id);
        Task<bool> UpdateSessionAsync(int id, SessionUpdateDto dto);
        Task<bool> DeleteSessionAsync(int id);
    }
}
