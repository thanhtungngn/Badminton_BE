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
        private readonly IPlayerPaymentRepository _playerPaymentRepo;
        private readonly ISessionPaymentRepository _sessionPaymentRepo;
        private readonly IPaymentService _paymentService;

        public SessionService(ISessionRepository repo, IPlayerPaymentRepository playerPaymentRepo, ISessionPaymentRepository sessionPaymentRepo, IPaymentService paymentService)
        {
            _repo = repo;
            _playerPaymentRepo = playerPaymentRepo;
            _sessionPaymentRepo = sessionPaymentRepo;
            _paymentService = paymentService;
        }

        public async Task<SessionWithPlayersDto?> GetSessionDetailAsync(int id)
        {
            var s = await _repo.GetByIdWithPlayersAsync(id);
            if (s == null) return null;

            var dto = new SessionWithPlayersDto
            {
                Id = s.Id.ToString(),
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
                    // determine price based on member gender and configured session prices
                    decimal price = 0m;
                    if (sessionPayment != null)
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
                        Price = price
                    });
                }
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
    }
}
