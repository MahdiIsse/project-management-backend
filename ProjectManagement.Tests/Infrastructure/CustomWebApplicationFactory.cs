using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProjectManagement.Infrastructure.Data;

namespace ProjectManagement.Tests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.ConfigureTestServices(services =>
    {
      services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
      services.RemoveAll<DbContextOptions<AppDbContext>>();
      services.RemoveAll<DbContextOptions>();
      services.RemoveAll<AppDbContext>();

      services.RemoveAll<Microsoft.AspNetCore.Identity.EntityFrameworkCore.UserStore<Microsoft.AspNetCore.Identity.IdentityUser>>();
      services.RemoveAll<Microsoft.AspNetCore.Identity.EntityFrameworkCore.RoleStore<Microsoft.AspNetCore.Identity.IdentityRole>>();

      var databaseName = $"TestDatabase_{Guid.NewGuid()}";
      services.AddDbContext<AppDbContext>(options =>
      {
        options.UseInMemoryDatabase(databaseName: databaseName);
        options.EnableSensitiveDataLogging();
      });

      services.AddAuthentication(options =>
      {
        options.DefaultAuthenticateScheme = "Test";
        options.DefaultChallengeScheme = "Test";
        options.DefaultScheme = "Test";
      })
      .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
    });

    builder.ConfigureAppConfiguration((context, config) =>
    {
      config.AddInMemoryCollection(new Dictionary<string, string?>
      {
        { "Jwt:Key", "ThisIsATestKeyThatIsLongEnoughForHmacSha256Algorithm"},
        { "Jwt:Issuer", "TestIssuer"},
        { "Jwt:Audience", "TestAudience"}
      });
    });

    builder.UseEnvironment("Testing");
  }
}
