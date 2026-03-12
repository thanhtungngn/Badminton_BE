using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Badminton_BE.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> GetAll();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
        Task<int> SaveChangesAsync();
    }
}
