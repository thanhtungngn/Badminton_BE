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

        public SessionService(ISessionRepository repo)
        {
            _repo = repo;
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

            existing.Title = dto.Title;
            existing.Description = dto.Description;
            existing.StartTime = dto.StartTime;
            existing.EndTime = dto.EndTime;
            existing.Address = dto.Address;
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
