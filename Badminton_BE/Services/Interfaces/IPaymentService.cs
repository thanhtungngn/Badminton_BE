using System.Collections.Generic;
using System.Threading.Tasks;
using Badminton_BE.DTOs;
using Badminton_BE.Models;

namespace Badminton_BE.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<SessionPayment?> SetSessionPricesAsync(int sessionId, decimal priceMale, decimal priceFemale);
        Task<IEnumerable<PlayerPaymentReadDto>> GeneratePlayerPaymentsForSessionAsync(int sessionId);
        Task<PlayerPaymentReadDto?> EnsurePlayerPaymentForSessionPlayerAsync(int sessionPlayerId);
        Task<PlayerPaymentReadDto?> PayPlayerPaymentAsync(int playerPaymentId, decimal amount);
        Task<PlayerPaymentReadDto?> PayBySessionPlayerIdAsync(int sessionPlayerId, decimal amount);
        Task<PlayerPaymentReadDto?> UpdateAmountDueAsync(int sessionPlayerId, decimal newAmountDue);
        Task<PlayerPaymentReadDto?> ConfirmPlayerPaymentAsync(int sessionPlayerId);
    }
}
