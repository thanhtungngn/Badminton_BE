using System.Threading.Tasks;
using Badminton_BE.DTOs;
using Badminton_BE.Models;

namespace Badminton_BE.Services.Interfaces
{
    public interface ISessionPlayerService
    {
        Task<SessionPlayerReadDto?> AddMemberToSessionAsync(SessionPlayerCreateDto dto);
        Task<SessionPlayerReadDto?> GetByIdAsync(int id);
        Task<bool> ChangeStatusAsync(int id, SessionPlayerStatus status);
        Task<bool> RemoveAsync(int id);
    }
}
