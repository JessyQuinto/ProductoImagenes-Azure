namespace ProductoImagenes.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string BlobUrl { get; set; }
        public string ContentType { get; set; }
        public DateTime UploadedAt { get; set; }
    }

}
