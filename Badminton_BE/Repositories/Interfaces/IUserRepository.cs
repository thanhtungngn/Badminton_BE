using System.Threading.Tasks;
using Badminton_BE.Models;

namespace Badminton_BE.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<AppUser>
    {
        Task<AppUser?> GetByNormalizedUsernameAsync(string normalizedUsername);
        Task<AppUser?> GetByPhoneNumberAsync(string phoneNumber);
        Task<AppUser?> GetByEmailAsync(string email);
    }
}
