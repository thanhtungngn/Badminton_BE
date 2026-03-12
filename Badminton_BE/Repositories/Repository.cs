using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Badminton_BE.Data;

namespace Badminton_BE.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _db;
        public Repository(AppDbContext db) => _db = db;

        public virtual IQueryable<T> GetAll()
            => _db.Set<T>().AsNoTracking();

        public virtual async Task<T?> GetByIdAsync(int id)
            => await _db.Set<T>().FindAsync(id).AsTask();

        public virtual async Task AddAsync(T entity)
            => await _db.Set<T>().AddAsync(entity);

        public virtual void Update(T entity)
            => _db.Set<T>().Update(entity);

        public virtual void Remove(T entity)
            => _db.Set<T>().Remove(entity);

        public virtual Task<int> SaveChangesAsync()
            => _db.SaveChangesAsync();
    }
}
