using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Badminton_BE.Models;

namespace Badminton_BE.Repositories
{
    public interface ISessionRepository : IRepository<Session>
    {
        Task<IEnumerable<Session>> GetByDateRangeAsync(DateTime start, DateTime end);
        Task<Session?> GetByIdWithPlayersAsync(int id);
    }
}
