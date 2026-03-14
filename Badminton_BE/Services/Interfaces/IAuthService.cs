using System.Threading.Tasks;
using Badminton_BE.DTOs;

namespace Badminton_BE.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto dto);
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto);
        Task LogoutAsync(int userId, string jti, DateTime expiresAt);
        Task<UserProfileReadDto?> GetProfileAsync(int userId);
        Task<bool> UpdateProfileAsync(int userId, UserProfileUpdateDto dto);
    }
}
