using Fundo.Applications.WebApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using System.Threading.Tasks;

namespace Fundo.Applications.WebApi.Health
{
    public class DbHealthCheck : IHealthCheck
    {
        private readonly AppDbContext _db;

        public DbHealthCheck(AppDbContext db)
        {
            _db = db;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            if (!_db.Database.IsRelational())
            {
                return HealthCheckResult.Healthy("Non-relational provider");
            }

            var canConnect = await _db.Database.CanConnectAsync(cancellationToken);

            return canConnect
                ? HealthCheckResult.Healthy("Database connection OK")
                : HealthCheckResult.Unhealthy("Database connection failed");
        }
    }
}
