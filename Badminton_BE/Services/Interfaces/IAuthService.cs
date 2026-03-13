using System.Threading.Tasks;
using Badminton_BE.DTOs;

namespace Badminton_BE.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto dto);
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto);
    }
}
