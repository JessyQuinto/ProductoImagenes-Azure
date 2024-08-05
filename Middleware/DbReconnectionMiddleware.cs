using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductoImagenes.Data;

namespace ProductoImagenes.Middleware
{
    public class DbReconnectionMiddleware
    {
        private readonly RequestDelegate _next;

        public DbReconnectionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ProductoDbContext dbContext, ILogger<DbReconnectionMiddleware> logger)
        {
            try
            {
                if (!await dbContext.Database.CanConnectAsync())
                {
                    logger.LogWarning("Database connection lost. Attempting to reconnect...");
                    await dbContext.Database.OpenConnectionAsync();
                    logger.LogInformation("Successfully reconnected to the database.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to reconnect to the database.");
            }

            await _next(context);
        }
    }
}