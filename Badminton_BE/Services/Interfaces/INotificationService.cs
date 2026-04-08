using System.Threading.Tasks;

namespace Badminton_BE.Services.Interfaces
{
    public interface INotificationService
    {
        Task TriggerPriceChangedAsync(int sessionId, decimal priceMale, decimal priceFemale);
        Task TriggerPaymentRecordedAsync(int sessionPlayerId);
        Task TriggerUnpaidReminderAsync(int sessionId);
    }
}
