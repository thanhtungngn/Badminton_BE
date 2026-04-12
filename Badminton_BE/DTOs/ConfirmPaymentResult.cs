namespace Badminton_BE.DTOs
{
    /// <summary>
    /// Result of a payment confirmation attempt, indicating whether a state transition actually occurred.
    /// </summary>
    public record ConfirmPaymentResult(PlayerPaymentReadDto? Dto, bool WasTransitioned);
}
