using System.Threading.Tasks;
using Badminton_BE.Models;

namespace Badminton_BE.Repositories.Interfaces
{
    public interface IRevokedTokenRepository : IRepository<RevokedToken>
    {
        Task<RevokedToken?> GetByJtiAsync(string jti);
        Task<bool> IsRevokedAsync(string jti);
    }
}
