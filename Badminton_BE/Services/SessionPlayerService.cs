using System.Threading.Tasks;
using Badminton_BE.DTOs;
using Badminton_BE.Models;
using Badminton_BE.Repositories;

namespace Badminton_BE.Services
{
    public class SessionPlayerService : ISessionPlayerService
    {
        private readonly ISessionPlayerRepository _repo;
        private readonly ISessionRepository _sessionRepo;
        private readonly IMemberRepository _memberRepo;

        public SessionPlayerService(ISessionPlayerRepository repo, ISessionRepository sessionRepo, IMemberRepository memberRepo)
        {
            _repo = repo;
            _sessionRepo = sessionRepo;
            _memberRepo = memberRepo;
        }

        public async Task<SessionPlayerReadDto?> AddMemberToSessionAsync(SessionPlayerCreateDto dto)
        {
            // validate session exists
            var session = await _sessionRepo.GetByIdAsync(dto.SessionId);
            if (session == null) return null;

            // validate member exists
            var member = await _memberRepo.GetByIdAsync(dto.MemberId);
            if (member == null) return null;

            // prevent duplicate entry for same session and member
            var existing = await _repo.GetBySessionAndMemberAsync(dto.SessionId, dto.MemberId);
            if (existing != null) return null;

            // prevent joining overlapping upcoming/ongoing sessions
            if (session.StartTime != default && session.EndTime != default)
            {
                var hasOverlap = await _repo.HasOverlappingSessionAsync(dto.MemberId, session.StartTime, session.EndTime);
                if (hasOverlap) return null;
            }

            var sp = new SessionPlayer
            {
                SessionId = dto.SessionId,
                MemberId = dto.MemberId,
                Status = dto.Status
            };

            await _repo.AddAsync(sp);
            await _repo.SaveChangesAsync();

            var created = await _repo.GetByIdWithIncludesAsync(sp.Id);
            if (created == null)
            {
                return null;
            }

            return new SessionPlayerReadDto
            {
                Id = created.Id,
                SessionId = created.SessionId,
                MemberId = created.MemberId,
                Level = created.Member?.Level.ToString() ?? string.Empty,
                EloPoint = created.Member?.PlayerRanking?.EloPoint,
                Status = created.Status,
                CreatedDate = created.CreatedDate,
                UpdatedDate = created.UpdatedDate
            };
        }

        public async Task<bool> ChangeStatusAsync(int id, SessionPlayerStatus status)
        {
            var sp = await _repo.GetByIdWithIncludesAsync(id);
            if (sp == null) return false;

            sp.Status = status;
            _repo.Update(sp);
            await _repo.SaveChangesAsync();
            return true;
        }

        public async Task<SessionPlayerReadDto?> GetByIdAsync(int id)
        {
            var sp = await _repo.GetByIdWithIncludesAsync(id);
            if (sp == null) return null;

            return new SessionPlayerReadDto
            {
                Id = sp.Id,
                SessionId = sp.SessionId,
                MemberId = sp.MemberId,
                Level = sp.Member?.Level.ToString() ?? string.Empty,
                EloPoint = sp.Member?.PlayerRanking?.EloPoint,
                Status = sp.Status,
                CreatedDate = sp.CreatedDate,
                UpdatedDate = sp.UpdatedDate
            };
        }

        public async Task<bool> RemoveAsync(int id)
        {
            var sp = await _repo.GetByIdWithIncludesAsync(id);
            if (sp == null) return false;

            _repo.Remove(sp);
            await _repo.SaveChangesAsync();
            return true;
        }
    }
}
                                         