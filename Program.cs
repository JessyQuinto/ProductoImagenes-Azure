using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ProductoImagenes.Data;
using ProductoImagenes.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar DbContext con SQL Server
builder.Services.AddDbContext<ProductoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductosDB")));

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
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProductoImagenes API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProductoImagenes API v1"));
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();

