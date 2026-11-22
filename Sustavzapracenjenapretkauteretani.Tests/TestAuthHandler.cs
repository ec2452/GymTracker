using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Sustavzapracenjenapretkauteretani.Tests;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Test handler always succeeds
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(
            Context.User,
            new AuthenticationProperties(),
            "TestScheme"
        )));
    }
}
