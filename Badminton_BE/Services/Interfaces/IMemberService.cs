using System.Collections.Generic;
using System.Threading.Tasks;
using Badminton_BE.DTOs;

namespace Badminton_BE.Services.Interfaces
{
    public interface IMemberService
    {
        Task<MemberReadDto> CreateMemberAsync(MemberCreateDto dto);
        Task<IEnumerable<MemberReadDto>> GetMembersAsync();
        Task<MemberReadDto?> GetMemberByIdAsync(int id);
        Task<MemberReadDto?> GetMemberByContactValueAsync(string contactValue);
        Task<MemberLookupDto?> GetMemberLookupByContactAsync(string contactValue);
        Task<bool> UpdateMemberAsync(int id, MemberUpdateDto dto);
        Task<bool> UpdateNicknameAsync(int id, string? nickname);
        Task<MemberLookupDto?> GetMemberByIdForLookupAsync(int memberId);
        Task<bool> DeleteMemberAsync(int id);
    }
}
