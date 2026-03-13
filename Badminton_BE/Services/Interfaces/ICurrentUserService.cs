namespace Badminton_BE.Services.Interfaces
{
    public interface ICurrentUserService
    {
        int? UserId { get; }
        bool IsAuthenticated { get; }
    }
}
