using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductoImagenes.Data;
using ProductoImagenes.Models;
using ProductoImagenes.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ProductoImagenes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly ProductoDbContext _context;
        private readonly IBlobService _blobService;
        private readonly ILogger<ProductosController> _logger;

        public ProductosController(ProductoDbContext context, IBlobService blobService, ILogger<ProductosController> logger)
        {
            _context = context;
            _blobService = blobService;
            _logger = logger;
        }

        // Subir un archivo y guardarlo en Azure Blob Storage
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                // Generar un nombre de archivo único
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

                // Subir archivo al blob y guardar la URL
                var blobUrl = await _blobService.UploadFileAsync(fileName, file.OpenReadStream(), file.ContentType);

                // Crear y guardar la información del producto
                var producto = new Producto
                {
                    Nombre = fileName,
                    BlobUrl = blobUrl,
                    ContentType = file.ContentType,
                    UploadedAt = DateTime.UtcNow
                };

                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "File uploaded successfully", ProductId = producto.Id, BlobUrl = producto.BlobUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(500, "Internal server error: Unable to upload file.");
            }
        }

        // Descargar un archivo del Azure Blob Storage por ID de producto
        [HttpGet("{id}")]
        public async Task<IActionResult> Download(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound("Producto no encontrado.");

            try
            {
                if (!await _blobService.FileExistsAsync(producto.Nombre))
                    return NotFound("Archivo no encontrado en Azure Blob Storage.");

                var fileStream = await _blobService.GetFileAsync(producto.Nombre);

                // Configurar la respuesta para forzar la descarga
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{producto.Nombre}\"");

                return File(fileStream, producto.ContentType, producto.Nombre);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file for product ID {ProductId}", id);
                return StatusCode(500, "Error interno del servidor al intentar descargar el archivo.");
            }
        }

        // Actualizar un archivo en Azure Blob Storage por ID de producto
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No se proporcionó ningún archivo.");

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound("Producto no encontrado.");

            try
            {
                // Generar un nombre de archivo único
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

                // Subir el nuevo archivo al blob y actualizar la URL
                var blobUrl = await _blobService.UploadFileAsync(fileName, file.OpenReadStream(), file.ContentType);

                // Eliminar el archivo antiguo si existe
                if (!string.IsNullOrEmpty(producto.Nombre))
                {
                    await _blobService.DeleteFileAsync(producto.Nombre);
                }

                // Actualizar la información del producto
                producto.Nombre = fileName;
                producto.BlobUrl = blobUrl;
                producto.ContentType = file.ContentType;
                producto.UploadedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { Message = "Archivo actualizado con éxito", ProductoId = producto.Id, NuevaBlobUrl = producto.BlobUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el archivo para el producto ID {ProductId}", id);
                return StatusCode(500, "Error interno del servidor al intentar actualizar el archivo.");
            }
        }

        // Eliminar un archivo del Azure Blob Storage por ID de producto
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound("Producto no encontrado.");

            try
            {
                // Eliminar archivo del blob y la información del producto
                await _blobService.DeleteFileAsync(producto.Nombre);
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Producto y archivo eliminados con éxito" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el producto y archivo para el ID {ProductId}", id);
                return StatusCode(500, "Error interno del servidor al intentar eliminar el producto y archivo.");
            }
        }

        // Listar todos los archivos en el contenedor de Azure Blob Storage
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Iniciando solicitud GET para obtener todos los productos");
            try
            {
                _logger.LogInformation("Intentando acceder al contexto de la base de datos");
                var productos = await _context.Productos.ToListAsync();
                _logger.LogInformation($"Se obtuvieron {productos.Count} productos");
                return Ok(productos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar los productos");
                return StatusCode(500, $"Error interno del servidor al intentar listar los productos: {ex.Message}");
            }
        }

        // Nuevo endpoint para probar la conexión a la base de datos
        [HttpGet("test-db-connection")]
        public async Task<IActionResult> TestDbConnection()
        {
            try
            {
                _logger.LogInformation("Probando conexión a la base de datos");
                await _context.Database.CanConnectAsync();
                _logger.LogInformation("Conexión a la base de datos exitosa");
                return Ok("Conexión a la base de datos exitosa");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al conectar a la base de datos");
                return StatusCode(500, $"Error al conectar a la base de datos: {ex.Message}");
            }
        }
    }
}