namespace Badminton_BE.Services
{
    public class DesignTimeCurrentUserService : ICurrentUserService
    {
        public int? UserId => null;
        public bool IsAuthenticated => false;
    }
}
