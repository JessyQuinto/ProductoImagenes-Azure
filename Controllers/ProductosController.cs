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

namespace ProductoImagenes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly ProductoDbContext _context;
        private readonly IBlobService _blobService;

        public ProductosController(ProductoDbContext context, IBlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                var blobUrl = await _blobService.UploadFileAsync(file.FileName, file.OpenReadStream(), file.ContentType);

                var producto = new Producto
                {
                    Nombre = file.FileName,
                    BlobUrl = blobUrl,
                    ContentType = file.ContentType,
                    UploadedAt = DateTime.UtcNow
                };

                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();

                return Ok(producto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Download(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound();

            try
            {
                var fileStream = await _blobService.GetFileAsync(producto.Nombre);
                return File(fileStream, producto.ContentType, producto.Nombre);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] IFormFile file)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound();

            try
            {
                var blobUrl = await _blobService.UploadFileAsync(file.FileName, file.OpenReadStream(), file.ContentType);

                producto.BlobUrl = blobUrl;
                producto.ContentType = file.ContentType;
                producto.UploadedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound();

            try
            {
                await _blobService.DeleteFileAsync(producto.Nombre);
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var blobs = new List<string>();
            await foreach (var blobItem in _blobService.GetBlobContainerClient().GetBlobsAsync())
            {
                blobs.Add(blobItem.Name);
            }
            return Ok(blobs);
        }
    }
}
