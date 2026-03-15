using System.Threading.Tasks;
using Badminton_BE.Models;

namespace Badminton_BE.Repositories.Interfaces
{
    public interface IPlayerRankingRepository : IRepository<PlayerRanking>
    {
        Task<PlayerRanking?> GetByMemberIdAsync(int memberId);
        Task<PlayerRanking?> GetByMemberIdWithRankingAsync(int memberId);
    }
}
