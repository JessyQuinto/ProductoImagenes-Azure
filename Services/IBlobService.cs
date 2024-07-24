using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace ProductoImagenes.Services
{
    public interface IBlobService
    {
        Task<string> UploadFileAsync(string fileName, Stream fileStream, string contentType);
        Task<bool> DeleteFileAsync(string fileName);
        Task<Stream> GetFileAsync(string fileName);
        BlobContainerClient GetBlobContainerClient();
    }
}
