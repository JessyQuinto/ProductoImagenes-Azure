using Azure.Storage.Blobs;
using System.IO;
using System.Threading.Tasks;

namespace ProductoImagenes.Services
{
    public interface IBlobService
    {
        // Subir un archivo al blob
        Task<string> UploadFileAsync(string fileName, Stream fileStream, string contentType);

        // Eliminar un archivo del blob
        Task<bool> DeleteFileAsync(string fileName);

        // Obtener un archivo del blob
        Task<Stream> GetFileAsync(string fileName);

        // Obtener el cliente del contenedor de blobs
        BlobContainerClient GetBlobContainerClient();
    }
}
