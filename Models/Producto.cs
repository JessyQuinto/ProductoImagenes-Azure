namespace ProductoImagenes.Models
{
    public class Producto
    {
        public int Id { get; set; } // ID del producto
        public string Nombre { get; set; } // Nombre del archivo
        public string BlobUrl { get; set; } // URL del archivo en Blob Storage
        public string ContentType { get; set; } // Tipo de contenido del archivo
        public DateTime UploadedAt { get; set; } // Fecha de subida del archivo
    }
}
