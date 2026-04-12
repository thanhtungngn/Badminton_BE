using Badminton_BE.Services;
using Badminton_BE.Services.Interfaces;

namespace Badminton_BE.Tests.Helpers;

public class TestCurrentUserService : ICurrentUserService
{
    public int? UserId { get; init; } = 1;
    public bool IsAuthenticated => UserId.HasValue;

    public static TestCurrentUserService WithUserId(int id) => new() { UserId = id };
    public static TestCurrentUserService Unauthenticated => new() { UserId = null };
}
