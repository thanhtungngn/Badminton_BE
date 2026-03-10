using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Badminton_BE.DTOs;
using Badminton_BE.Models;
using Badminton_BE.Repositories;

namespace Badminton_BE.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ISessionPaymentRepository _sessionPaymentRepo;
        private readonly IPlayerPaymentRepository _playerPaymentRepo;
        private readonly ISessionRepository _sessionRepo;
        private readonly ISessionPlayerRepository _sessionPlayerRepo;

        public PaymentService(
            ISessionPaymentRepository sessionPaymentRepo,
            IPlayerPaymentRepository playerPaymentRepo,
            ISessionRepository sessionRepo,
            ISessionPlayerRepository sessionPlayerRepo)
        {
            _sessionPaymentRepo = sessionPaymentRepo;
            _playerPaymentRepo = playerPaymentRepo;
            _sessionRepo = sessionRepo;
            _sessionPlayerRepo = sessionPlayerRepo;
        }

        public async Task<SessionPayment?> SetSessionPricesAsync(int sessionId, decimal priceMale, decimal priceFemale)
        {
            var session = await _sessionRepo.GetByIdAsync(sessionId);
            if (session == null) return null;

            var existing = await _sessionPaymentRepo.GetBySessionIdAsync(sessionId);
            if (existing != null)
            {
                existing.PriceMale = priceMale;
                existing.PriceFemale = priceFemale;
                _sessionPaymentRepo.Update(existing);
                await _sessionPaymentRepo.SaveChangesAsync();
                return existing;
            }

            var sp = new SessionPayment
            {
                SessionId = sessionId,
                PriceMale = priceMale,
                PriceFemale = priceFemale
            };

            await _sessionPaymentRepo.AddAsync(sp);
            await _sessionPaymentRepo.SaveChangesAsync();
            return sp;
        }

        public async Task<IEnumerable<PlayerPaymentReadDto>> GeneratePlayerPaymentsForSessionAsync(int sessionId)
        {
            var session = await _sessionRepo.GetByIdWithPlayersAsync(sessionId);
            if (session == null) return Enumerable.Empty<PlayerPaymentReadDto>();

            var spayment = await _sessionPaymentRepo.GetBySessionIdAsync(sessionId);
            if (spayment == null) return Enumerable.Empty<PlayerPaymentReadDto>();

            var created = new List<PlayerPaymentReadDto>();

            if (session.SessionPlayers == null) return created;

            foreach (var sp in session.SessionPlayers)
            {
                // skip if payment already exists for session player
                var existing = await _playerPaymentRepo.GetBySessionPlayerIdAsync(sp.Id);
                if (existing != null) continue;

                decimal due = 0m;
                if (sp.Member != null)
                {
                    // assume Gender enum names Male/Female
                    due = sp.Member.Gender.ToString().ToLowerInvariant().StartsWith("m") ? spayment.PriceMale : spayment.PriceFemale;
                }

                var pp = new PlayerPayment
                {
                    SessionPlayerId = sp.Id,
                    AmountDue = due,
                    AmountPaid = 0m,
                    PaidStatus = PaymentStatus.NotPaid
                };

                await _playerPaymentRepo.AddAsync(pp);
                await _playerPaymentRepo.SaveChangesAsync();

                created.Add(new PlayerPaymentReadDto
                {
                    Id = pp.Id,
                    SessionPlayerId = pp.SessionPlayerId,
                    AmountDue = pp.AmountDue,
                    AmountPaid = pp.AmountPaid,
                    PaidStatus = pp.PaidStatus.ToString(),
                    PaidAt = pp.PaidAt
                });
            }

            return created;
        }

        public async Task<PlayerPaymentReadDto?> PayPlayerPaymentAsync(int playerPaymentId, decimal amount)
        {
            var pp = await _playerPaymentRepo.GetByIdAsync(playerPaymentId);
            if (pp == null) return null;

            pp.AmountPaid += amount;
            if (pp.AmountPaid >= pp.AmountDue)
            {
                pp.PaidStatus = PaymentStatus.Paid;
                pp.PaidAt = System.DateTime.UtcNow;
            }
            else if (pp.AmountPaid > 0)
            {
                pp.PaidStatus = PaymentStatus.Partial;
            }

            _playerPaymentRepo.Update(pp);
            await _playerPaymentRepo.SaveChangesAsync();

            return new PlayerPaymentReadDto
            {
                Id = pp.Id,
                SessionPlayerId = pp.SessionPlayerId,
                AmountDue = pp.AmountDue,
                AmountPaid = pp.AmountPaid,
                PaidStatus = pp.PaidStatus.ToString(),
                PaidAt = pp.PaidAt
            };
        }
    }
}
