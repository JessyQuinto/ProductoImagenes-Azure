using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProductoImagenes.Services
{
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public BlobService(string connectionString, string containerName)
        {
            _blobServiceClient = new BlobServiceClient(connectionString ?? throw new ArgumentNullException(nameof(connectionString)));
            _containerName = containerName ?? throw new ArgumentNullException(nameof(containerName));
        }

        // Subir un archivo al blob
        public async Task<string> UploadFileAsync(string fileName, Stream fileStream, string contentType)
        {
            var containerClient = GetBlobContainerClient();
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            string cleanFileName = CleanFileName(fileName);
            var blobClient = containerClient.GetBlobClient(cleanFileName);

            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType });
            return blobClient.Uri.ToString();
        }

        // Eliminar un archivo del blob
        public async Task<bool> DeleteFileAsync(string fileName)
        {
            var containerClient = GetBlobContainerClient();
            var blobClient = containerClient.GetBlobClient(fileName);
            return await blobClient.DeleteIfExistsAsync();
        }

        // Obtener un archivo del blob
        public async Task<Stream> GetFileAsync(string fileName)
        {
            var containerClient = GetBlobContainerClient();
            var blobClient = containerClient.GetBlobClient(fileName);
            var downloadInfo = await blobClient.DownloadAsync();
            return downloadInfo.Value.Content;
        }

        // Obtener el cliente del contenedor de blobs
        public BlobContainerClient GetBlobContainerClient()
        {
            return _blobServiceClient.GetBlobContainerClient(_containerName);
        }

        // Limpiar el nombre del archivo de caracteres inválidos
        private string CleanFileName(string fileName)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return Regex.Replace(fileName, invalidRegStr, "_");
        }
    }
}
