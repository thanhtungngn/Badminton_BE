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
        private readonly IPlayerRankingRepository _playerRankingRepo;
        private readonly ISessionPlayerRepository _sessionPlayerRepo;
        private readonly IPlayerPaymentRepository _playerPaymentRepo;
        private readonly IPlayerRankingService _playerRankingService;
        private readonly IUserRepository _userRepo;

        public MemberService(
            IMemberRepository repo,
            IPlayerRankingRepository playerRankingRepo,
            ISessionPlayerRepository sessionPlayerRepo,
            IPlayerPaymentRepository playerPaymentRepo,
            IPlayerRankingService playerRankingService,
            IUserRepository userRepo)
        {
            _repo = repo;
            _playerRankingRepo = playerRankingRepo;
            _sessionPlayerRepo = sessionPlayerRepo;
            _playerPaymentRepo = playerPaymentRepo;
            _playerRankingService = playerRankingService;
            _userRepo = userRepo;
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
                        ContactValue = c.ContactValue.Trim(),
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

        public async Task<MemberLookupDto?> GetMemberLookupByContactAsync(string contactValue)
        {
            var normalizedContactValue = contactValue.Trim();

            var member = await _repo.GetByContactValueIgnoreFiltersAsync(normalizedContactValue);

            if (member == null)
            {
                return null;
            }

            var matchedContactValue = member.Contacts
                .Where(c => c.ContactValue == normalizedContactValue)
                .OrderByDescending(c => c.IsPrimary)
                .Select(c => c.ContactValue)
                .FirstOrDefault() ?? normalizedContactValue;

            var ranking = await _playerRankingRepo.GetByMemberIdWithRankingAsync(member.Id);
            var sessionPlayers = (await _sessionPlayerRepo.GetByMemberIdWithSessionAsync(member.Id)).ToList();
            var payments = (await _playerPaymentRepo.GetBySessionPlayerIdsAsync(sessionPlayers.Select(sp => sp.Id)))
                .ToDictionary(p => p.SessionPlayerId);

            var allSessionDtos = sessionPlayers
                .Where(sp => sp.Session != null)
                .Select(sp =>
                {
                    payments.TryGetValue(sp.Id, out var payment);
                    return new MemberLookupSessionDto
                    {
                        SessionId = sp.Session!.Id,
                        SessionPlayerId = sp.Id,
                        Title = sp.Session.Title,
                        StartTime = sp.Session.StartTime,
                        EndTime = sp.Session.EndTime,
                        Address = sp.Session.Address,
                        SessionStatus = sp.Session.Status.ToString(),
                        PlayerStatus = sp.Status.ToString(),
                        PaymentStatus = payment?.PaidStatus.ToString() ?? PaymentStatus.NotPaid.ToString(),
                        AmountDue = payment?.AmountDue,
                        AmountPaid = payment?.AmountPaid,
                        PaidAt = payment?.PaidAt
                    };
                })
                .ToList();

            // Build unpaid sessions grouped by session owner (UserId)
            var unpaidSps = sessionPlayers
                .Where(sp => sp.Session != null)
                .Where(sp =>
                {
                    payments.TryGetValue(sp.Id, out var p);
                    return p == null || p.PaidStatus != PaymentStatus.Paid;
                })
                .GroupBy(sp => sp.Session!.UserId)
                .ToList();

            var unpaidByUser = new List<UnpaidSessionsByOwnerDto>();
            foreach (var group in unpaidSps)
            {
                var owner = await _userRepo.GetByIdAsync(group.Key);
                var groupSessions = group.Select(sp =>
                {
                    payments.TryGetValue(sp.Id, out var payment);
                    return new MemberLookupSessionDto
                    {
                        SessionId = sp.Session!.Id,
                        SessionPlayerId = sp.Id,
                        Title = sp.Session.Title,
                        StartTime = sp.Session.StartTime,
                        EndTime = sp.Session.EndTime,
                        Address = sp.Session.Address,
                        SessionStatus = sp.Session.Status.ToString(),
                        PlayerStatus = sp.Status.ToString(),
                        PaymentStatus = payment?.PaidStatus.ToString() ?? PaymentStatus.NotPaid.ToString(),
                        AmountDue = payment?.AmountDue,
                        AmountPaid = payment?.AmountPaid,
                        PaidAt = payment?.PaidAt
                    };
                }).ToList();

                unpaidByUser.Add(new UnpaidSessionsByOwnerDto
                {
                    UserId = group.Key,
                    OwnerName = owner?.Name ?? owner?.Username ?? string.Empty,
                    BankAccountNumber = owner?.BankAccountNumber,
                    BankOwnerName = owner?.BankOwnerName,
                    BankName = owner?.BankName,
                    TotalAmountDue = groupSessions.Sum(s => s.AmountDue ?? 0m),
                    Sessions = groupSessions
                });
            }

            return new MemberLookupDto
            {
                MemberId = member.Id,
                Name = member.Name,
                ContactValue = matchedContactValue,
                Level = member.Level.ToString(),
                EloPoint = ranking?.EloPoint,
                RankingName = ranking?.Ranking?.Name,
                Sessions = allSessionDtos,
                UnpaidByUser = unpaidByUser
            };
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
                        ContactValue = c.ContactValue.Trim(),
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
