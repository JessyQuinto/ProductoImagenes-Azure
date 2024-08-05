using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductoImagenes.Data;

namespace ProductoImagenes.Middleware
{
    public class DbReconnectionFilter : IAsyncActionFilter
    {
        private readonly ProductoDbContext _dbContext;
        private readonly ILogger<DbReconnectionFilter> _logger;

        public DbReconnectionFilter(ProductoDbContext dbContext, ILogger<DbReconnectionFilter> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                if (!await _dbContext.Database.CanConnectAsync())
                {
                    _logger.LogWarning("Database connection lost. Attempting to reconnect...");
                    await _dbContext.Database.OpenConnectionAsync();
                    _logger.LogInformation("Successfully reconnected to the database.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reconnect to the database.");
            }

            await next();
        }
    }
}