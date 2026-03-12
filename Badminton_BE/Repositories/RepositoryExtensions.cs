using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Badminton_BE.Repositories.Interfaces;

namespace Badminton_BE.Repositories
{
    public static class RepositoryExtensions
    {
        public static Task<List<T>> GetAllAsync<T>(this IRepository<T> repo) where T : class
        {
            return repo.GetAll().ToListAsync();
        }
    }
}
