using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Fundo.Applications.WebApi.Security
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "ApiKey";
        public const string HeaderName = "X-Api-Key";

        private readonly IConfiguration _configuration;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration configuration)
            : base(options, logger, encoder, clock)
        {
            _configuration = configuration;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var configuredKey = _configuration["Authentication:ApiKey"];

            if (string.IsNullOrWhiteSpace(configuredKey))
            {
                return Task.FromResult(AuthenticateResult.Fail("API key authentication is not configured."));
            }

            if (!Request.Headers.TryGetValue(HeaderName, out var providedKey) || string.IsNullOrWhiteSpace(providedKey))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            if (!string.Equals(providedKey.ToString(), configuredKey))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "ApiKeyClient")
            };

            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
