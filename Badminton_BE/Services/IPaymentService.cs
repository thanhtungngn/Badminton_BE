using System.Collections.Generic;
using System.Threading.Tasks;
using Badminton_BE.DTOs;
using Badminton_BE.Models;

namespace Badminton_BE.Services
{
    public interface IPaymentService
    {
        Task<SessionPayment?> SetSessionPricesAsync(int sessionId, decimal priceMale, decimal priceFemale);
        Task<IEnumerable<PlayerPaymentReadDto>> GeneratePlayerPaymentsForSessionAsync(int sessionId);
        Task<PlayerPaymentReadDto?> PayPlayerPaymentAsync(int playerPaymentId, decimal amount);
    }
}
