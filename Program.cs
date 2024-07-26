using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ProductoImagenes.Data;
using ProductoImagenes.Services;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ProductoImagenes.IntegrationTests")]

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Configurar DbContext con SQL Server y habilitar resiliencia a errores transitorios
        services.AddDbContext<ProductoDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("ProductosDB"),
            sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));

        // Registrar BlobServiceClient
        var storageConnectionString = Configuration.GetConnectionString("StorageAccount");
        if (string.IsNullOrEmpty(storageConnectionString))
        {
            throw new ArgumentNullException("StorageAccount connection string is not configured.");
        }
        services.AddSingleton(x => new BlobServiceClient(storageConnectionString));

        // Registrar BlobService
        var containerName = Configuration["BlobService:ContainerName"];
        services.AddSingleton<IBlobService>(sp =>
            new BlobService(storageConnectionString, containerName));

        // Configurar controladores
        services.AddControllers();

        // Configurar Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProductoImagenes API", Version = "v1" });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
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
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}