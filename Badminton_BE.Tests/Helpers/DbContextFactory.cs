using Badminton_BE.Data;
using Badminton_BE.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Badminton_BE.Tests.Helpers;

public static class DbContextFactory
{
    public static AppDbContext Create(ICurrentUserService? userService = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options, userService ?? new TestCurrentUserService());
    }
}
