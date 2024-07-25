using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ProductoImagenes.Data;
using ProductoImagenes.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar DbContext con SQL Server y habilitar resiliencia a errores transitorios
builder.Services.AddDbContext<ProductoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductosDB"),
    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));

// Registrar BlobServiceClient
var storageConnectionString = builder.Configuration.GetConnectionString("StorageAccount");
if (string.IsNullOrEmpty(storageConnectionString))
{
    throw new ArgumentNullException("StorageAccount connection string is not configured.");
}
builder.Services.AddSingleton(x => new BlobServiceClient(storageConnectionString));

// Registrar BlobService
var containerName = builder.Configuration["BlobService:ContainerName"];
builder.Services.AddSingleton<IBlobService>(sp =>
    new BlobService(storageConnectionString, containerName));

// Configurar controladores
builder.Services.AddControllers();

// Configurar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProductoImagenes API", Version = "v1" });
});

var app = builder.Build();

// Configurar el pipeline de solicitud HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProductoImagenes API v1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();