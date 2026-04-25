using System.Collections.Generic;
using System.Threading.Tasks;
using Badminton_BE.Models;

namespace Badminton_BE.Repositories.Interfaces
{
    public interface IMemberRepository : IRepository<Member>
    {
        Task<Member?> GetByIdWithContactsAsync(int id);
        Task<IEnumerable<Member>> GetAllWithContactsAsync();
        Task<Member?> GetByContactValueAsync(string contactValue);
        Task<Member?> GetByContactValueIgnoreFiltersAsync(string contactValue);
        Task<IEnumerable<Member>> GetAllByContactValueIgnoreFiltersAsync(string contactValue);
        Task<Member?> GetByPhoneNumberForUserIgnoreFiltersAsync(int userId, string phoneNumber);
        Task<IEnumerable<Member>> GetMembersWithoutPlayerRankingAsync();
    }
}
