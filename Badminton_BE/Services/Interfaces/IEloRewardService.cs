using System.Threading.Tasks;
using Badminton_BE.Models;

namespace Badminton_BE.Services.Interfaces
{
    public interface IEloRewardService
    {
        Task ApplyAsync(SessionMatch match);
        Task ReverseAsync(SessionMatch match);
    }
}
