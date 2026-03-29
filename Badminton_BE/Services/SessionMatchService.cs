using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Badminton_BE.DTOs;
using Badminton_BE.Models;
using Badminton_BE.Repositories;

namespace Badminton_BE.Services
{
    public class SessionMatchService : ISessionMatchService
    {
        private readonly ISessionMatchRepository _matchRepo;
        private readonly ISessionRepository _sessionRepo;
        private readonly ISessionPlayerRepository _sessionPlayerRepo;
        private readonly IEloRewardService _eloRewardService;

        public SessionMatchService(
            ISessionMatchRepository matchRepo,
            ISessionRepository sessionRepo,
            ISessionPlayerRepository sessionPlayerRepo,
            IEloRewardService eloRewardService)
        {
            _matchRepo = matchRepo;
            _sessionRepo = sessionRepo;
            _sessionPlayerRepo = sessionPlayerRepo;
            _eloRewardService = eloRewardService;
        }

        public async Task<IEnumerable<SessionMatchReadDto>> GetBySessionIdAsync(int sessionId)
        {
            var matches = await _matchRepo.GetBySessionIdAsync(sessionId);
            return matches.Select(MapToReadDto);
        }

        public async Task<SessionMatchReadDto?> GetByIdAsync(int sessionId, int matchId)
        {
            var match = await _matchRepo.GetBySessionAndMatchIdAsync(sessionId, matchId);
            return match == null ? null : MapToReadDto(match);
        }

        public async Task<SessionMatchReadDto?> CreateAsync(int sessionId, SessionMatchUpsertDto dto)
        {
            var session = await _sessionRepo.GetByIdAsync(sessionId);
            if (session == null || !IsValidWinner(dto))
            {
                return null;
            }

            var teamPlayers = await LoadValidatedTeamPlayersAsync(sessionId, dto.TeamAPlayerIds, dto.TeamBPlayerIds);
            if (teamPlayers == null)
            {
                return null;
            }

            var match = new SessionMatch
            {
                SessionId = sessionId,
                UserId = session.UserId,
                TeamAScore = dto.TeamAScore,
                TeamBScore = dto.TeamBScore,
                Winner = dto.Winner,
                Players = teamPlayers
            };

            await _matchRepo.AddAsync(match);
            await _matchRepo.SaveChangesAsync();

            var created = await _matchRepo.GetByIdWithPlayersAsync(match.Id);
            if (created == null) return null;

            await _eloRewardService.ApplyAsync(created);
            await _matchRepo.SaveChangesAsync();

            return MapToReadDto(created);
        }

        public async Task<SessionMatchReadDto?> UpdateAsync(int sessionId, int matchId, SessionMatchUpsertDto dto)
        {
            var match = await _matchRepo.GetBySessionAndMatchIdAsync(sessionId, matchId);
            if (match == null || !IsValidWinner(dto))
            {
                return null;
            }

            var teamPlayers = await LoadValidatedTeamPlayersAsync(sessionId, dto.TeamAPlayerIds, dto.TeamBPlayerIds);
            if (teamPlayers == null)
            {
                return null;
            }

            // Reverse old Elo before replacing players
            await _eloRewardService.ReverseAsync(match);

            match.TeamAScore = dto.TeamAScore;
            match.TeamBScore = dto.TeamBScore;
            match.Winner = dto.Winner;
            match.Players.Clear();
            foreach (var player in teamPlayers)
            {
                match.Players.Add(player);
            }

            _matchRepo.Update(match);
            await _matchRepo.SaveChangesAsync();

            var updated = await _matchRepo.GetByIdWithPlayersAsync(match.Id);
            if (updated == null) return null;

            await _eloRewardService.ApplyAsync(updated);
            await _matchRepo.SaveChangesAsync();

            return MapToReadDto(updated);
        }

        public async Task<bool> DeleteAsync(int sessionId, int matchId)
        {
            var match = await _matchRepo.GetBySessionAndMatchIdAsync(sessionId, matchId);
            if (match == null)
            {
                return false;
            }

            await _eloRewardService.ReverseAsync(match);
            _matchRepo.Remove(match);
            await _matchRepo.SaveChangesAsync();
            return true;
        }

        private async Task<List<SessionMatchPlayer>?> LoadValidatedTeamPlayersAsync(int sessionId, List<int> teamAPlayerIds, List<int> teamBPlayerIds)
        {
            if (!HasValidTeamSize(teamAPlayerIds) || !HasValidTeamSize(teamBPlayerIds))
            {
                return null;
            }

            var allPlayerIds = teamAPlayerIds.Concat(teamBPlayerIds).ToList();
            if (allPlayerIds.Count != allPlayerIds.Distinct().Count())
            {
                return null;
            }

            var result = new List<SessionMatchPlayer>();

            foreach (var id in teamAPlayerIds)
            {
                var sessionPlayer = await _sessionPlayerRepo.GetByIdWithIncludesAsync(id);
                if (sessionPlayer == null || sessionPlayer.SessionId != sessionId)
                {
                    return null;
                }

                result.Add(new SessionMatchPlayer
                {
                    UserId = sessionPlayer.UserId,
                    SessionPlayerId = sessionPlayer.Id,
                    Team = MatchTeam.TeamA
                });
            }

            foreach (var id in teamBPlayerIds)
            {
                var sessionPlayer = await _sessionPlayerRepo.GetByIdWithIncludesAsync(id);
                if (sessionPlayer == null || sessionPlayer.SessionId != sessionId)
                {
                    return null;
                }

                result.Add(new SessionMatchPlayer
                {
                    UserId = sessionPlayer.UserId,
                    SessionPlayerId = sessionPlayer.Id,
                    Team = MatchTeam.TeamB
                });
            }

            return result;
        }

        private static bool HasValidTeamSize(List<int> playerIds)
        {
            return playerIds != null && playerIds.Count >= 1 && playerIds.Count <= 2;
        }

        private static bool IsValidWinner(SessionMatchUpsertDto dto)
        {
            if (dto.Winner == MatchWinner.Pending)
            {
                return true;
            }

            if (dto.TeamAScore == dto.TeamBScore)
            {
                return dto.Winner == MatchWinner.Draw;
            }

            return dto.TeamAScore > dto.TeamBScore
                ? dto.Winner == MatchWinner.TeamA
                : dto.Winner == MatchWinner.TeamB;
        }

        private static SessionMatchReadDto MapToReadDto(SessionMatch match)
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
                    .Select(MapToPlayerDto)
                    .ToList(),
                TeamBPlayers = match.Players
                    .Where(p => p.Team == MatchTeam.TeamB)
                    .OrderBy(p => p.Id)
                    .Select(MapToPlayerDto)
                    .ToList(),
                CreatedDate = match.CreatedDate,
                UpdatedDate = match.UpdatedDate
            };
        }

        private static SessionMatchPlayerReadDto MapToPlayerDto(SessionMatchPlayer player)
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
