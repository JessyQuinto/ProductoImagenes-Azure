using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ProductoImagenes.Data;
using ProductoImagenes.Services;
using Azure.Storage.Blobs;

var builder = WebApplication.CreateBuilder(args);

// Configurar Azure Key Vault
var keyVaultEndpoint = builder.Configuration["VaultUri"];
if (!string.IsNullOrEmpty(keyVaultEndpoint))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultEndpoint),
        new DefaultAzureCredential());
}

// Configurar DbContext
var dbConnectionString = builder.Configuration["ProductosDB"];
if (string.IsNullOrEmpty(dbConnectionString))
{
    throw new ArgumentNullException("ProductosDB connection string is not configured.");
}
builder.Services.AddDbContext<ProductoDbContext>(options =>
    options.UseSqlServer(dbConnectionString,
    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));

// Registrar BlobServiceClient
var storageConnectionString = builder.Configuration["StorageAccount"];
if (string.IsNullOrEmpty(storageConnectionString))
{
    throw new ArgumentNullException("StorageAccount connection string is not configured.");
}
builder.Services.AddSingleton(x => new BlobServiceClient(storageConnectionString));

// Registrar BlobService
var containerName = builder.Configuration["BlobService:ContainerName"];
if (string.IsNullOrEmpty(containerName))
{
    throw new ArgumentNullException("BlobService:ContainerName is not configured.");
}
builder.Services.AddSingleton<IBlobService>(sp =>
    new BlobService(storageConnectionString, containerName));

// Agregar servicios al contenedor.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProductoImagenes API", Version = "v1" });
});

// Configurar CORS si es necesario
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("http://example.com")
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});

// Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Configurar el pipeline de solicitudes HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProductoImagenes API v1"));
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");
app.UseAuthorization();

app.MapControllers();

app.Run();