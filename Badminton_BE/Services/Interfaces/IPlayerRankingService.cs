using System.Threading.Tasks;
using Badminton_BE.Models;

namespace Badminton_BE.Services.Interfaces
{
    public interface IPlayerRankingService
    {
        Task SyncForMemberAsync(Member member);
        Task<int> BackfillMissingRankingsAsync();
    }
}
