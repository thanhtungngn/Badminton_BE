using System.Collections.Generic;
using System.Threading.Tasks;
using Badminton_BE.Models;

namespace Badminton_BE.Repositories.Interfaces
{
    public interface IRankingRepository : IRepository<Ranking>
    {
        Task<IEnumerable<Ranking>> GetAllAsync();
    }
}
