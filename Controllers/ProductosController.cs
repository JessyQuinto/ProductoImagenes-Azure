using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductoImagenes.Data;
using ProductoImagenes.Models;
using ProductoImagenes.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ProductoImagenes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly ProductoDbContext _context;
        private readonly BlobService _blobService;

        public ProductosController(ProductoDbContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var url = await _blobService.UploadFileAsync(fileName, stream, file.ContentType);

                var producto = new Producto
                {
                    Nombre = fileName,
                    BlobUrl = url,
                    ContentType = file.ContentType,
                    UploadedAt = DateTime.UtcNow
                };

                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();

                return Ok(producto);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound();

            var stream = await _blobService.GetFileAsync(producto.Nombre);
            return File(stream, producto.ContentType, Path.GetFileName(producto.BlobUrl));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromForm] IFormFile file)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var url = await _blobService.UploadFileAsync(fileName, stream, file.ContentType);
                producto.BlobUrl = url;
                producto.ContentType = file.ContentType;
                producto.UploadedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound();

            await _blobService.DeleteFileAsync(producto.Nombre);
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            return NoContent();
        }


    }
}