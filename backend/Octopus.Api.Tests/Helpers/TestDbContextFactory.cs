using System;
using Microsoft.EntityFrameworkCore;
using Octopus.Api.Data;

namespace Octopus.Api.Tests.Helpers;

public static class TestDbContextFactory
{
    public static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}