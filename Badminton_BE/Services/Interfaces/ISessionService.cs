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
        Task<IEnumerable<SessionReadDto>> GetParticipantSessionsAsync(string phoneNumber);
        Task<SessionReadDto?> GetSessionByIdAsync(int id);
        Task<SessionWithPlayersDto?> GetSessionDetailAsync(int id);
        Task<PublicSessionRegistrationResultDto> RegisterPublicAsync(int sessionId, PublicSessionRegistrationDto dto);
        Task<bool> UpdateSessionAsync(int id, SessionUpdateDto dto);
        Task<bool> DeleteSessionAsync(int id);
    }
}
