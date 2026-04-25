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
        private readonly ISessionMatchRepository _sessionMatchRepo;
        private readonly IPlayerPaymentRepository _playerPaymentRepo;
        private readonly IPlayerRankingService _playerRankingService;
        private readonly IUserRepository _userRepo;
        private readonly ICurrentUserService _currentUserService;

        public MemberService(
            IMemberRepository repo,
            IPlayerRankingRepository playerRankingRepo,
            ISessionPlayerRepository sessionPlayerRepo,
            ISessionMatchRepository sessionMatchRepo,
            IPlayerPaymentRepository playerPaymentRepo,
            IPlayerRankingService playerRankingService,
            IUserRepository userRepo,
            ICurrentUserService currentUserService)
        {
            _repo = repo;
            _playerRankingRepo = playerRankingRepo;
            _sessionPlayerRepo = sessionPlayerRepo;
            _sessionMatchRepo = sessionMatchRepo;
            _playerPaymentRepo = playerPaymentRepo;
            _playerRankingService = playerRankingService;
            _userRepo = userRepo;
            _currentUserService = currentUserService;
        }

        public async Task<MemberReadDto> CreateMemberAsync(MemberCreateDto dto)
        {
            // Check if any contact value already exists in any host's list
            Member? existingDataMember = null;
            if (dto.Contacts != null && dto.Contacts.Any())
            {
                foreach (var c in dto.Contacts)
                {
                    existingDataMember = await _repo.GetByContactValueIgnoreFiltersAsync(c.ContactValue);
                    if (existingDataMember != null) break;
                }
            }

            var member = new Member
            {
                Name = dto.Name,
                Gender = dto.Gender,
                Level = dto.Level,
                JoinDate = dto.JoinDate,
                Avatar = dto.Avatar
            };

            // If we found existing data and the DTO didn't specify certain fields, use the existing ones
            if (existingDataMember != null)
            {
                if (string.IsNullOrWhiteSpace(member.Name)) member.Name = existingDataMember.Name;
                if (member.Gender == 0) member.Gender = existingDataMember.Gender;
                if (member.Level == MemberLevel.Newbie) member.Level = existingDataMember.Level;
                if (string.IsNullOrWhiteSpace(member.Avatar)) member.Avatar = existingDataMember.Avatar;
            }

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
            var dtos = new List<MemberReadDto>();
            foreach (var m in members)
            {
                var dto = MapToReadDto(m);
                var stats = await BuildMatchStatsAsync(m.Id, _currentUserService.UserId);
                dto.Wins = stats.Wins;
                dto.Losses = stats.Losses;
                dto.Draws = stats.Draws;
                dto.WinRate = stats.WinRate;
                dtos.Add(dto);
            }
            return dtos;
        }

        public async Task<MemberReadDto?> GetMemberByIdAsync(int id)
        {
            var m = await _repo.GetByIdWithContactsAsync(id);
            if (m == null) return null;

            var dto = MapToReadDto(m);
            var stats = await BuildMatchStatsAsync(m.Id, _currentUserService.UserId);
            dto.Wins = stats.Wins;
            dto.Losses = stats.Losses;
            dto.Draws = stats.Draws;
            dto.WinRate = stats.WinRate;
            dto.UnpaidByUser = await BuildUnpaidByUserAsync(m.Id, _currentUserService.UserId);
            return dto;
        }

        public async Task<MemberReadDto?> GetMemberByContactValueAsync(string contactValue)
        {
            var m = await _repo.GetByContactValueAsync(contactValue);
            if (m == null) return null;

            var dto = MapToReadDto(m);
            var stats = await BuildMatchStatsAsync(m.Id, _currentUserService.UserId);
            dto.Wins = stats.Wins;
            dto.Losses = stats.Losses;
            dto.Draws = stats.Draws;
            dto.WinRate = stats.WinRate;
            dto.UnpaidByUser = await BuildUnpaidByUserAsync(m.Id, _currentUserService.UserId);
            return dto;
        }

        public async Task<MemberLookupDto?> GetMemberLookupByContactAsync(string contactValue)
        {
            if (string.IsNullOrWhiteSpace(contactValue)) return null;
            var normalizedContactValue = contactValue.Trim();

            // Find ALL member records with this contact value across all hosts
            var members = (await _repo.GetAllByContactValueIgnoreFiltersAsync(normalizedContactValue)).ToList();

            if (!members.Any())
            {
                // Fallback: Check if this is a registered user who hasn't been added to any sessions yet
                var user = await _userRepo.GetByPhoneNumberAsync(normalizedContactValue);
                if (user != null)
                {
                    return new MemberLookupDto
                    {
                        Name = user.Name ?? user.Username,
                        ContactValue = normalizedContactValue,
                        Level = MemberLevel.Newbie.ToString(),
                        Wins = 0,
                        Losses = 0,
                        Draws = 0,
                        WinRate = 0,
                        Sessions = new List<MemberLookupSessionDto>(),
                        UnpaidByUser = new List<UnpaidSessionsByOwnerDto>()
                    };
                }
                return null;
            }

            // Use the primary member record for basic display info (Name, Level, etc.)
            // Prefer one that has more sessions or is recently updated
            var primaryMember = members.OrderByDescending(m => m.SessionPlayers.Count)
                                       .ThenByDescending(m => m.UpdatedDate ?? m.CreatedDate)
                                       .First();

            var matchedContactValue = primaryMember.Contacts
                .Where(c => c.ContactValue == normalizedContactValue)
                .OrderByDescending(c => c.IsPrimary)
                .Select(c => c.ContactValue)
                .FirstOrDefault() ?? normalizedContactValue;

            // Aggregate sessions and rankings from all matching member records
            var allSessionPlayers = new List<SessionPlayer>();
            foreach (var m in members)
            {
                var sps = await _sessionPlayerRepo.GetByMemberIdWithSessionAsync(m.Id);
                allSessionPlayers.AddRange(sps);
            }

            var sessionPlayerIds = allSessionPlayers.Select(sp => sp.Id).ToList();
            var payments = (await _playerPaymentRepo.GetBySessionPlayerIdsAsync(sessionPlayerIds))
                .ToDictionary(p => p.SessionPlayerId);

            var allSessionDtos = allSessionPlayers
                .Where(sp => sp.Session != null)
                .OrderByDescending(sp => sp.Session!.StartTime)
                .Select(sp => MapToLookupSessionDto(sp, payments))
                .ToList();

            // Aggregate Match Stats
            var totalWins = 0;
            var totalLosses = 0;
            var totalDraws = 0;
            var totalMatches = 0;

            foreach (var m in members)
            {
                var stats = await BuildMatchStatsAsync(m.Id);
                totalWins += stats.Wins;
                totalLosses += stats.Losses;
                totalDraws += stats.Draws;
                
                // Recalculate matches from stats to be accurate
                var mTotal = stats.Wins + stats.Losses + stats.Draws;
                totalMatches += mTotal;
            }

            var winRate = totalMatches > 0 ? decimal.Round(totalWins * 100m / totalMatches, 2) : 0m;

            // Get Ranking (highest one found)
            var ranking = members
                .Select(m => m.PlayerRanking)
                .Where(pr => pr != null)
                .OrderByDescending(pr => pr.EloPoint)
                .FirstOrDefault();

            return new MemberLookupDto
            {
                MemberId = primaryMember.Id,
                Name = primaryMember.Name,
                Nickname = primaryMember.Nickname,
                ContactValue = matchedContactValue,
                Level = primaryMember.Level.ToString(),
                EloPoint = ranking?.EloPoint,
                RankingName = ranking?.Ranking?.Name,
                Wins = totalWins,
                Losses = totalLosses,
                Draws = totalDraws,
                WinRate = winRate,
                Sessions = allSessionDtos,
                UnpaidByUser = await BuildUnpaidByUserAsync(allSessionPlayers, payments)
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

        public async Task<bool> UpdateNicknameAsync(int id, string? nickname)
        {
            var member = await _repo.GetByIdAsync(id);
            if (member == null) return false;

            member.Nickname = nickname?.Trim();
            member.UpdatedDate = DateTime.UtcNow;
            _repo.Update(member);
            await _repo.SaveChangesAsync();
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
                Nickname = m.Nickname,
                Gender = m.Gender,
                Level = m.Level,
                JoinDate = m.JoinDate,
                Avatar = m.Avatar,
                CreatedDate = m.CreatedDate,
                UpdatedDate = m.UpdatedDate,
                EloPoint = m.PlayerRanking?.EloPoint,
                RankingName = m.PlayerRanking?.Ranking?.Name,
                Contacts = m.Contacts.Select(c => new DTOs.ContactReadDto
                {
                    Id = c.Id,
                    ContactType = c.ContactType,
                    ContactValue = c.ContactValue,
                    IsPrimary = c.IsPrimary
                }).ToList()
            };
        }

        private async Task<MemberMatchStatsDto> BuildMatchStatsAsync(int memberId, int? ownerUserId = null)
        {
            var matches = (await _sessionMatchRepo.GetByMemberIdAsync(memberId, ownerUserId))
                .Where(m => m.Winner != MatchWinner.Pending)
                .ToList();

            if (matches.Count == 0)
            {
                return new MemberMatchStatsDto();
            }

            var wins = 0;
            var losses = 0;
            var draws = 0;

            foreach (var match in matches)
            {
                var player = match.Players.FirstOrDefault(p => p.SessionPlayer?.MemberId == memberId);
                if (player == null) continue;

                if (match.Winner == MatchWinner.Draw) { draws++; continue; }

                var isWin = (player.Team == MatchTeam.TeamA && match.Winner == MatchWinner.TeamA)
                    || (player.Team == MatchTeam.TeamB && match.Winner == MatchWinner.TeamB);

                if (isWin) wins++;
                else losses++;
            }

            return new MemberMatchStatsDto
            {
                Wins = wins,
                Losses = losses,
                Draws = draws,
                WinRate = decimal.Round(wins * 100m / matches.Count, 2)
            };
        }

        private async Task<List<UnpaidSessionsByOwnerDto>> BuildUnpaidByUserAsync(int memberId, int? ownerUserId = null)
        {
            var sessionPlayers = (await _sessionPlayerRepo.GetByMemberIdWithSessionAsync(memberId)).ToList();
            var payments = (await _playerPaymentRepo.GetBySessionPlayerIdsAsync(sessionPlayers.Select(sp => sp.Id)))
                .ToDictionary(p => p.SessionPlayerId);

            return await BuildUnpaidByUserAsync(sessionPlayers, payments, ownerUserId);
        }

        private async Task<List<UnpaidSessionsByOwnerDto>> BuildUnpaidByUserAsync(
            List<SessionPlayer> sessionPlayers,
            Dictionary<int, PlayerPayment> payments,
            int? ownerUserId = null)
        {
            var unpaidByOwnerGroups = sessionPlayers
                .Where(sp => sp.Session != null)
                .Where(sp => !ownerUserId.HasValue || sp.Session!.UserId == ownerUserId.Value)
                .Where(sp => !payments.TryGetValue(sp.Id, out var payment) || payment.PaidStatus != PaymentStatus.Paid)
                .GroupBy(sp => sp.Session!.UserId)
                .ToList();

            var unpaidByUser = new List<UnpaidSessionsByOwnerDto>();
            foreach (var group in unpaidByOwnerGroups)
            {
                var owner = await _userRepo.GetByIdAsync(group.Key);
                var groupSessions = group
                    .Select(sp => MapToLookupSessionDto(sp, payments))
                    .ToList();

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

            return unpaidByUser;
        }

        private static MemberLookupSessionDto MapToLookupSessionDto(SessionPlayer sessionPlayer, Dictionary<int, PlayerPayment> payments)
        {
            payments.TryGetValue(sessionPlayer.Id, out var payment);

            return new MemberLookupSessionDto
            {
                SessionId = sessionPlayer.Session!.Id,
                SessionPlayerId = sessionPlayer.Id,
                Title = sessionPlayer.Session.Title,
                StartTime = sessionPlayer.Session.StartTime,
                EndTime = sessionPlayer.Session.EndTime,
                Address = sessionPlayer.Session.Address,
                SessionStatus = sessionPlayer.Session.Status.ToString(),
                PlayerStatus = sessionPlayer.Status.ToString(),
                PaymentStatus = payment?.PaidStatus.ToString() ?? PaymentStatus.NotPaid.ToString(),
                AmountDue = payment?.AmountDue,
                AmountPaid = payment?.AmountPaid,
                PaidAt = payment?.PaidAt
            };
        }
    }
}
