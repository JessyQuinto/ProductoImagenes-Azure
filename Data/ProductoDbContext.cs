using Microsoft.EntityFrameworkCore;
using ProductoImagenes.Models;

namespace ProductoImagenes.Data
{
    public class ProductoDbContext : DbContext
    {
        public ProductoDbContext(DbContextOptions<ProductoDbContext> options) : base(options) { }

        // DbSet para la entidad Producto
        public DbSet<Producto> Productos { get; set; }
    }
}
