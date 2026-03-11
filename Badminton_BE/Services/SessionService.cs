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

        public SessionService(ISessionRepository repo, IPlayerPaymentRepository playerPaymentRepo)
        {
            _repo = repo;
            _playerPaymentRepo = playerPaymentRepo;
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
                    dto.Players.Add(new PlayerResponseDto
                    {
                        Id = sp.Id.ToString(),
                        MemberId = sp.Member.Id.ToString(),
                        Name = sp.Member.Name,
                        Contact = contact,
                        Level = sp.Member.Level.ToString(),
                        PaidStatus = payment != null ? (payment.PaidStatus == PaymentStatus.Paid) : (bool?)null
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
                MaxPlayerPerCourt = dto.MaxPlayerPerCourt
            };
            // set created date explicitly
            s.CreatedDate = DateTime.UtcNow;

            await _repo.AddAsync(s);
            await _repo.SaveChangesAsync();

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
                MaxPlayerPerCourt = s.MaxPlayerPerCourt
            };
        }

        public async Task<IEnumerable<SessionReadDto>> GetSessionsAsync()
        {
            var sessions = await _repo.GetAllAsync();
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
                MaxPlayerPerCourt = s.MaxPlayerPerCourt
            });
        }

        public async Task<SessionReadDto?> GetSessionByIdAsync(int id)
        {
            var s = await _repo.GetByIdAsync(id);
            if (s == null) return null;

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
                MaxPlayerPerCourt = s.MaxPlayerPerCourt
            };
        }

        public async Task<bool> UpdateSessionAsync(int id, SessionUpdateDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

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

            return true;
        }
    }
}
