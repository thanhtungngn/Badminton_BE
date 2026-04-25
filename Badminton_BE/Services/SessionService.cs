using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Badminton_BE.DTOs;
using Badminton_BE.Models;
using Badminton_BE.Repositories;

namespace Badminton_BE.Services
{
    public class SessionService : ISessionService
    {
        private readonly ISessionRepository _repo;
        private readonly IMemberRepository _memberRepo;
        private readonly ISessionPlayerRepository _sessionPlayerRepo;
        private readonly IPlayerPaymentRepository _playerPaymentRepo;
        private readonly ISessionPaymentRepository _sessionPaymentRepo;
        private readonly IPaymentService _paymentService;
        private readonly IPlayerRankingService _playerRankingService;

        public SessionService(
            ISessionRepository repo,
            IMemberRepository memberRepo,
            ISessionPlayerRepository sessionPlayerRepo,
            IPlayerPaymentRepository playerPaymentRepo,
            ISessionPaymentRepository sessionPaymentRepo,
            IPaymentService paymentService,
            IPlayerRankingService playerRankingService)
        {
            _repo = repo;
            _memberRepo = memberRepo;
            _sessionPlayerRepo = sessionPlayerRepo;
            _playerPaymentRepo = playerPaymentRepo;
            _sessionPaymentRepo = sessionPaymentRepo;
            _paymentService = paymentService;
            _playerRankingService = playerRankingService;
        }

        public async Task<SessionWithPlayersDto?> GetSessionDetailAsync(int id)
        {
            var s = await _repo.GetByIdWithPlayersAsync(id);
            if (s == null) return null;

            var dto = new SessionWithPlayersDto
            {
                Id = s.Id.ToString(),
                Title = s.Title,
                Description = s.Description,
                Address = s.Address,
                DateCreated = s.StartTime,
                Courts = s.NumberOfCourts,
                MaxPlayersPerCourt = s.MaxPlayerPerCourt,
                Status = s.Status.ToString().ToLowerInvariant(),
                CreatedAt = s.CreatedDate,
                OwnerQrCode = s.PaymentQrCodeUrl
            };

            // load session-level prices once
            var sessionPayment = await _sessionPaymentRepo.GetBySessionIdAsync(s.Id);

            if (s.SessionPlayers != null)
            {
                foreach (var sp in s.SessionPlayers)
                {
                    if (sp.Member == null) continue;

                    // pick primary contact if present, otherwise first contact value
                    string contact = string.Empty;
                    if (sp.Member.Contacts != null && sp.Member.Contacts.Count > 0)
                    {
                        var primary = sp.Member.Contacts.FirstOrDefault(c => c.IsPrimary);
                        var first = sp.Member.Contacts.FirstOrDefault();
                        contact = (primary ?? first)?.ContactValue ?? string.Empty;
                    }

                    var payment = await _playerPaymentRepo.GetBySessionPlayerIdAsync(sp.Id);
                    // use the player's individual AmountDue if a payment record exists,
                    // otherwise fall back to the session-level price based on gender
                    decimal price = 0m;
                    if (payment != null)
                    {
                        price = payment.AmountDue;
                    }
                    else if (sessionPayment != null)
                    {
                        price = sp.Member.Gender == Gender.Male ? sessionPayment.PriceMale : sessionPayment.PriceFemale;
                    }

                    dto.Players.Add(new PlayerResponseDto
                    {
                        Id = sp.Id.ToString(),
                        MemberId = sp.Member.Id.ToString(),
                        Name = sp.Member.Name,
                        Contact = contact,
                        Level = sp.Member.Level.ToString(),
                        EloPoint = sp.Member.PlayerRanking?.EloPoint,
                        PaidStatus = payment != null ? (payment.PaidStatus == PaymentStatus.Paid) : (bool?)null,
                        PlayerPaymentId = payment?.Id,
                        Price = price,
                        Status = (int)sp.Status
                    });
                }
            }

            if (s.Matches != null)
            {
                dto.Matches = s.Matches
                    .OrderByDescending(m => m.CreatedDate)
                    .Select(MapToMatchDto)
                    .ToList();
            }

            return dto;
        }

        public async Task<SessionReadDto> CreateSessionAsync(SessionCreateDto dto)
        {
            var s = new Session
            {
                Title = dto.Title,
                Description = dto.Description,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Address = dto.Address,
                Status = dto.Status,
                NumberOfCourts = dto.NumberOfCourts,
                MaxPlayerPerCourt = dto.MaxPlayerPerCourt,
                PaymentQrCodeUrl = dto.PaymentQrCodeUrl
            };
            // set created date explicitly
            s.CreatedDate = DateTime.UtcNow;

            await _repo.AddAsync(s);
            await _repo.SaveChangesAsync();

            // create session-level prices
            var sp = new SessionPayment
            {
                SessionId = s.Id,
                PriceMale = dto.PriceMale,
                PriceFemale = dto.PriceFemale
            };

            await _sessionPaymentRepo.AddAsync(sp);
            await _sessionPaymentRepo.SaveChangesAsync();

            return new SessionReadDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Address = s.Address,
                Status = s.Status,
                NumberOfCourts = s.NumberOfCourts,
                MaxPlayerPerCourt = s.MaxPlayerPerCourt,
                PriceMale = sp.PriceMale,
                PriceFemale = sp.PriceFemale
            };
        }

        public async Task<IEnumerable<SessionReadDto>> GetSessionsAsync()
        {
            var sessions = _repo.GetAll();
            return sessions.Select(s => new SessionReadDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Address = s.Address,
                Status = s.Status,
                NumberOfCourts = s.NumberOfCourts,
                MaxPlayerPerCourt = s.MaxPlayerPerCourt,
                RegisteredPlayers = s.SessionPlayers != null ? s.SessionPlayers.Count : 0
            }).ToList();
        }

        public async Task<IEnumerable<SessionReadDto>> GetActiveSessionsAsync()
        {
            var sessions = _repo.GetAll();
            // Only include Upcoming and OnGoing sessions for dashboard
            var active = sessions.Where(s => s.Status == SessionStatus.Upcoming || s.Status == SessionStatus.OnGoing);
            return active.Select(s => new SessionReadDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Address = s.Address,
                Status = s.Status,
                NumberOfCourts = s.NumberOfCourts,
                MaxPlayerPerCourt = s.MaxPlayerPerCourt,
                RegisteredPlayers = s.SessionPlayers != null ? s.SessionPlayers.Count : 0
            }).ToList();
        }

        public async Task<IEnumerable<SessionReadDto>> GetParticipantSessionsAsync(string phoneNumber)
        {
            var sessions = await _repo.GetByParticipantPhoneNumberAsync(phoneNumber);
            return sessions.Select(s => new SessionReadDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Address = s.Address,
                Status = s.Status,
                NumberOfCourts = s.NumberOfCourts,
                MaxPlayerPerCourt = s.MaxPlayerPerCourt,
                RegisteredPlayers = s.SessionPlayers != null ? s.SessionPlayers.Count : 0
            }).ToList();
        }

        public async Task<SessionReadDto?> GetSessionByIdAsync(int id)
        {
            var s = await _repo.GetByIdAsync(id);
            if (s == null) return null;

            // try to include session-level prices if available
            var spayment = await _sessionPaymentRepo.GetBySessionIdAsync(id);

            return new SessionReadDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Address = s.Address,
                Status = s.Status,
                NumberOfCourts = s.NumberOfCourts,
                MaxPlayerPerCourt = s.MaxPlayerPerCourt,
                PriceMale = spayment?.PriceMale,
                PriceFemale = spayment?.PriceFemale
            };
        }

        public async Task<PublicSessionRegistrationResultDto> RegisterPublicAsync(int sessionId, PublicSessionRegistrationDto dto)
        {
            var session = await _repo.GetByIdAsync(sessionId);
            if (session == null)
            {
                return new PublicSessionRegistrationResultDto
                {
                    RegistrationStatus = PublicSessionRegistrationStatus.SessionNotFound,
                    SessionId = sessionId,
                    Name = dto.Name?.Trim() ?? string.Empty,
                    PhoneNumber = dto.PhoneNumber?.Trim() ?? string.Empty,
                    Message = "Session not found."
                };
            }

            var normalizedName = dto.Name.Trim();
            var normalizedPhoneNumber = dto.PhoneNumber.Trim();

            var member = await _memberRepo.GetByPhoneNumberForUserIgnoreFiltersAsync(session.UserId, normalizedPhoneNumber);
            var isNewMember = false;

            if (member == null)
            {
                var newMember = new Member
                {
                    UserId = session.UserId,
                    Name = normalizedName,
                    Gender = dto.Gender,
                    Level = MemberLevel.LowerIntermediate,
                    JoinDate = DateTime.UtcNow,
                    Contacts = new List<Contact>
                    {
                        new Contact
                        {
                            UserId = session.UserId,
                            ContactType = ContactType.Phone,
                            ContactValue = normalizedPhoneNumber,
                            IsPrimary = true
                        }
                    }
                };

                await _memberRepo.AddAsync(newMember);
                await _memberRepo.SaveChangesAsync();
                await _playerRankingService.SyncForMemberAsync(newMember);

                member = await _memberRepo.GetByIdWithContactsAsync(newMember.Id) ?? newMember;
                isNewMember = true;
            }

            var existingRegistration = await _sessionPlayerRepo.GetBySessionAndMemberAsync(sessionId, member.Id);
            if (existingRegistration != null)
            {
                return new PublicSessionRegistrationResultDto
                {
                    RegistrationStatus = PublicSessionRegistrationStatus.AlreadyRegistered,
                    SessionId = sessionId,
                    SessionPlayerId = existingRegistration.Id,
                    MemberId = member.Id,
                    Name = member.Name,
                    PhoneNumber = normalizedPhoneNumber,
                    Level = member.Level.ToString(),
                    EloPoint = member.PlayerRanking?.EloPoint,
                    Message = "This phone number is already registered for the session."
                };
            }

            if (session.StartTime != default && session.EndTime != default)
            {
                var hasOverlap = await _sessionPlayerRepo.HasOverlappingSessionAsync(member.Id, session.StartTime, session.EndTime);
                if (hasOverlap)
                {
                    return new PublicSessionRegistrationResultDto
                    {
                        RegistrationStatus = PublicSessionRegistrationStatus.OverlappingSession,
                        SessionId = sessionId,
                        MemberId = member.Id,
                        Name = member.Name,
                        PhoneNumber = normalizedPhoneNumber,
                        Level = member.Level.ToString(),
                        EloPoint = member.PlayerRanking?.EloPoint,
                        Message = "This player already joined another overlapping active session."
                    };
                }
            }

            var sessionPlayer = new SessionPlayer
            {
                UserId = session.UserId,
                SessionId = sessionId,
                MemberId = member.Id,
                Status = SessionPlayerStatus.Joined
            };

            await _sessionPlayerRepo.AddAsync(sessionPlayer);
            await _sessionPlayerRepo.SaveChangesAsync();

            if (session.Status == SessionStatus.OnGoing)
            {
                await _paymentService.EnsurePlayerPaymentForSessionPlayerAsync(sessionPlayer.Id);
            }

            var createdRegistration = await _sessionPlayerRepo.GetByIdWithIncludesAsync(sessionPlayer.Id);

            return new PublicSessionRegistrationResultDto
            {
                RegistrationStatus = PublicSessionRegistrationStatus.Registered,
                IsNewMember = isNewMember,
                SessionId = sessionId,
                SessionPlayerId = sessionPlayer.Id,
                MemberId = member.Id,
                Name = member.Name,
                PhoneNumber = normalizedPhoneNumber,
                Level = createdRegistration?.Member?.Level.ToString() ?? member.Level.ToString(),
                EloPoint = createdRegistration?.Member?.PlayerRanking?.EloPoint ?? member.PlayerRanking?.EloPoint,
                Message = isNewMember ? "Registration completed and a new member was created." : "Registration completed successfully."
            };
        }

        public async Task<bool> UpdateSessionAsync(int id, SessionUpdateDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            var previousStatus = existing.Status;

            existing.Title = dto.Title ?? existing.Title;
            existing.Description = dto.Title ?? dto.Description;
            existing.StartTime = dto.StartTime ?? existing.StartTime;
            existing.EndTime = dto.EndTime ?? existing.EndTime;
            existing.Address = dto.Address ?? existing.Address;
            existing.Status = dto.Status;
            existing.NumberOfCourts = dto.NumberOfCourts;
            existing.MaxPlayerPerCourt = dto.MaxPlayerPerCourt;

            // set updated date
            existing.UpdatedDate = DateTime.UtcNow;

            _repo.Update(existing);
            await _repo.SaveChangesAsync();

            if (previousStatus != SessionStatus.OnGoing && existing.Status == SessionStatus.OnGoing)
            {
                await _paymentService.GeneratePlayerPaymentsForSessionAsync(existing.Id);
            }

            return true;
        }

        public async Task<bool> DeleteSessionAsync(int id)
        {
            var existing = await _repo.GetByIdWithPlayersAsync(id);
            if (existing == null) return false;

            // Remove player payments for this session (if any)
            var playerPayments = await _playerPaymentRepo.GetBySessionIdAsync(id);
            if (playerPayments != null)
            {
                foreach (var pp in playerPayments)
                {
                    _playerPaymentRepo.Remove(pp);
                }
                await _playerPaymentRepo.SaveChangesAsync();
            }

            // Remove session-level payment if present
            var spayment = await _sessionPaymentRepo.GetBySessionIdAsync(id);
            if (spayment != null)
            {
                _sessionPaymentRepo.Remove(spayment);
                await _sessionPaymentRepo.SaveChangesAsync();
            }

            // Remove the session itself
            _repo.Remove(existing);
            await _repo.SaveChangesAsync();

            return true;
        }

        private static SessionMatchReadDto MapToMatchDto(SessionMatch match)
        {
            return new SessionMatchReadDto
            {
                Id = match.Id,
                SessionId = match.SessionId,
                TeamAScore = match.TeamAScore,
                TeamBScore = match.TeamBScore,
                Winner = match.Winner,
                TeamAPlayers = match.Players
                    .Where(p => p.Team == MatchTeam.TeamA)
                    .OrderBy(p => p.Id)
                    .Select(MapToMatchPlayerDto)
                    .ToList(),
                TeamBPlayers = match.Players
                    .Where(p => p.Team == MatchTeam.TeamB)
                    .OrderBy(p => p.Id)
                    .Select(MapToMatchPlayerDto)
                    .ToList(),
                CreatedDate = match.CreatedDate,
                UpdatedDate = match.UpdatedDate
            };
        }

        private static SessionMatchPlayerReadDto MapToMatchPlayerDto(SessionMatchPlayer player)
        {
            return new SessionMatchPlayerReadDto
            {
                SessionPlayerId = player.SessionPlayerId,
                MemberId = player.SessionPlayer?.MemberId ?? 0,
                Name = player.SessionPlayer?.Member?.Name ?? string.Empty,
                Level = player.SessionPlayer?.Member?.Level.ToString() ?? string.Empty,
                EloPoint = player.SessionPlayer?.Member?.PlayerRanking?.EloPoint
            };
        }
    }
}
