using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Badminton_BE.Data;
using Badminton_BE.Models;

namespace Badminton_BE.Repositories
{
    public class RankingRepository : Repository<Ranking>, IRankingRepository
    {
        public RankingRepository(AppDbContext db) : base(db) { }

        public async Task<IEnumerable<Ranking>> GetAllAsync()
        {
            return await _db.Rankings
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
