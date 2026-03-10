using System.Collections.Generic;
using System.Threading.Tasks;
using Badminton_BE.DTOs;

namespace Badminton_BE.Services
{
    public interface ISessionService
    {
        Task<SessionReadDto> CreateSessionAsync(SessionCreateDto dto);
        Task<IEnumerable<SessionReadDto>> GetSessionsAsync();
        Task<SessionReadDto?> GetSessionByIdAsync(int id);
        Task<SessionWithPlayersDto?> GetSessionDetailAsync(int id);
        Task<bool> UpdateSessionAsync(int id, SessionUpdateDto dto);
    }
}
