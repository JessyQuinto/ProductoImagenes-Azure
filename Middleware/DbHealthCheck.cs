using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductoImagenes.Data;

namespace ProductoImagenes.HealthChecks
{
    public class DbHealthCheck : IHealthCheck
    {
        private readonly ProductoDbContext _dbContext;
        private readonly ILogger<DbHealthCheck> _logger;

        public DbHealthCheck(ProductoDbContext dbContext, ILogger<DbHealthCheck> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (await _dbContext.Database.CanConnectAsync(cancellationToken))
                {
                    return HealthCheckResult.Healthy("Database connection is healthy.");
                }

                _logger.LogWarning("Database connection lost. Attempting to reconnect...");
                await _dbContext.Database.OpenConnectionAsync(cancellationToken);
                _logger.LogInformation("Successfully reconnected to the database.");
                return HealthCheckResult.Healthy("Database reconnection successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to the database.");
                return HealthCheckResult.Unhealthy("Database connection failed.", ex);
            }
        }
    }
}