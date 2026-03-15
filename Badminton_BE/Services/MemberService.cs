using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Badminton_BE.DTOs;
using Badminton_BE.Models;
using Badminton_BE.Repositories;

namespace Badminton_BE.Services
{
    public class MemberService : IMemberService
    {
        private readonly IMemberRepository _repo;
        private readonly IPlayerRankingService _playerRankingService;

        public MemberService(IMemberRepository repo, IPlayerRankingService playerRankingService)
        {
            _repo = repo;
            _playerRankingService = playerRankingService;
        }

        public async Task<MemberReadDto> CreateMemberAsync(MemberCreateDto dto)
        {
            var member = new Member
            {
                Name = dto.Name,
                Gender = dto.Gender,
                Level = dto.Level,
                JoinDate = dto.JoinDate,
                Avatar = dto.Avatar
            };

            if (dto.Contacts != null)
            {
                foreach (var c in dto.Contacts)
                {
                    member.Contacts.Add(new Contact
                    {
                        ContactType = c.ContactType,
                        ContactValue = c.ContactValue,
                        IsPrimary = c.IsPrimary
                    });
                }
            }

            await _repo.AddAsync(member);
            await _repo.SaveChangesAsync();
            await _playerRankingService.SyncForMemberAsync(member);

            return MapToReadDto(member);
        }

        public async Task<IEnumerable<MemberReadDto>> GetMembersAsync()
        {
            var members = await _repo.GetAllWithContactsAsync();
            return members.Select(MapToReadDto);
        }

        public async Task<MemberReadDto?> GetMemberByIdAsync(int id)
        {
            var m = await _repo.GetByIdWithContactsAsync(id);
            if (m == null) return null;
            return MapToReadDto(m);
        }

        public async Task<MemberReadDto?> GetMemberByContactValueAsync(string contactValue)
        {
            var m = await _repo.GetByContactValueAsync(contactValue);
            if (m == null) return null;
            return MapToReadDto(m);
        }

        public async Task<bool> UpdateMemberAsync(int id, MemberUpdateDto dto)
        {
            var existing = await _repo.GetByIdWithContactsAsync(id);
            if (existing == null) return false;

            existing.Name = dto.Name;
            existing.Gender = dto.Gender;
            existing.Level = dto.Level;
            existing.JoinDate = dto.JoinDate;
            existing.Avatar = dto.Avatar;

            // Replace contacts: remove existing and add new
            existing.Contacts.Clear();
            if (dto.Contacts != null)
            {
                foreach (var c in dto.Contacts)
                {
                    existing.Contacts.Add(new Contact
                    {
                        ContactType = c.ContactType,
                        ContactValue = c.ContactValue,
                        IsPrimary = c.IsPrimary
                    });
                }
            }

            _repo.Update(existing);
            await _repo.SaveChangesAsync();
            await _playerRankingService.SyncForMemberAsync(existing);

            return true;
        }

        public async Task<bool> DeleteMemberAsync(int id)
        {
            var existing = await _repo.GetByIdWithContactsAsync(id);
            if (existing == null) return false;

            _repo.Remove(existing);
            await _repo.SaveChangesAsync();
            return true;
        }

        private MemberReadDto MapToReadDto(Member m)
        {
            return new MemberReadDto
            {
                Id = m.Id,
                Name = m.Name,
                Gender = m.Gender,
                Level = m.Level,
                JoinDate = m.JoinDate,
                Avatar = m.Avatar,
                CreatedDate = m.CreatedDate,
                UpdatedDate = m.UpdatedDate,
                Contacts = m.Contacts.Select(c => new DTOs.ContactReadDto
                {
                    Id = c.Id,
                    ContactType = c.ContactType,
                    ContactValue = c.ContactValue,
                    IsPrimary = c.IsPrimary
                }).ToList()
            };
        }
    }
}
