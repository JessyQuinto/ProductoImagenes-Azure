using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using ProductoImagenes.Data;
using ProductoImagenes.Services;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Azure.Security.KeyVault.Secrets;
using Azure.Extensions.AspNetCore.Configuration.Secrets;

var builder = WebApplication.CreateBuilder(args);

// Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();

var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();

// Configurar Azure Key Vault
if (builder.Environment.IsDevelopment())
{
    var keyVaultUrl = builder.Configuration["KeyVault:KeyVaultURL"];
    var clientId = builder.Configuration["KeyVault:ClientId"];
    var clientSecret = builder.Configuration["KeyVault:ClientSecret"];
    var directoryId = builder.Configuration["KeyVault:DirectoryId"];

    if (!string.IsNullOrEmpty(keyVaultUrl))
    {
        try
        {
            logger.LogInformation("Attempting to configure Azure Key Vault");
            var credential = new ClientSecretCredential(directoryId, clientId, clientSecret);
            var client = new SecretClient(new Uri(keyVaultUrl), credential);
            builder.Configuration.AddAzureKeyVault(client, new AzureKeyVaultConfigurationOptions());
            logger.LogInformation("Successfully configured Azure Key Vault");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error configuring Azure Key Vault");
        }
    }
}

// Configurar Azure AD
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

// Configurar DbContext
var dbConnectionString = builder.Configuration["ProductosDB"];
logger.LogInformation($"DbConnectionString: {dbConnectionString}");
if (string.IsNullOrEmpty(dbConnectionString))
{
    logger.LogWarning("ProductosDB connection string is not configured.");
}
else
{
    builder.Services.AddDbContext<ProductoDbContext>(options =>
        options.UseSqlServer(dbConnectionString,
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));
    logger.LogInformation("DbContext configured successfully");
}

// Registrar BlobServiceClient
var storageConnectionString = builder.Configuration["StorageAccountfeedbackgeneral"];
logger.LogInformation($"StorageConnectionString: {storageConnectionString}");
if (string.IsNullOrEmpty(storageConnectionString))
{
    logger.LogWarning("StorageAccountfeedbackgeneral connection string is not configured.");
}
else
{
    builder.Services.AddSingleton(x => new BlobServiceClient(storageConnectionString));
    logger.LogInformation("BlobServiceClient registered successfully");
}

// Registrar BlobService
var containerName = builder.Configuration["BlobService:ContainerName"];
logger.LogInformation($"ContainerName: {containerName}");
if (string.IsNullOrEmpty(containerName))
{
    logger.LogWarning("BlobService:ContainerName is not configured.");
}
else
{
    builder.Services.AddSingleton<IBlobService>(sp =>
        new BlobService(storageConnectionString, containerName));
    logger.LogInformation("BlobService registered successfully");
}

// Agregar servicios al contenedor.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configurar Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProductoImagenes API", Version = "v1" });
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri(builder.Configuration["Swagger:AuthorizationUrl"]),
                TokenUrl = new Uri(builder.Configuration["Swagger:TokenUrl"]),
                Scopes = new Dictionary<string, string>
                {
                    { builder.Configuration["Swagger:Scope"], "Access as user" }
                }
            }
        }
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
            },
            new[] { builder.Configuration["Swagger:Scope"] }
        }
    });
});

// Configurar CORS
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins(corsOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});

var app = builder.Build();

// Configurar el pipeline de solicitudes HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Habilitar Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProductoImagenes API v1");
    c.OAuthClientId(builder.Configuration["AzureAd:ClientId"]);
    c.OAuthUsePkce();
});

app.UseCors("AllowSpecificOrigin");

// Usar autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Agregar middleware de logging
app.Use(async (context, next) =>
{
    logger.LogInformation($"Request {context.Request.Method} {context.Request.Path} started");
    await next();
    logger.LogInformation($"Request {context.Request.Method} {context.Request.Path} completed with status code {context.Response.StatusCode}");
});

app.MapControllers();

try
{
    logger.LogInformation("Starting application");
    app.Run();
    logger.LogInformation("Application stopped");
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Application terminated unexpectedly");
}