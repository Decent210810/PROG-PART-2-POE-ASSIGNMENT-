using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.API.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    }

    public class BlobStorageService : IBlobStorageService
    {
        private readonly string _connectionString;
        private readonly string _containerName = "raceday-images";

        public BlobStorageService(IConfiguration configuration)
        {
            _connectionString = configuration.GetSection("AzureBlobStorage").Value
                ?? throw new ArgumentNullException("AzureBlobStorage connection string missing");
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);

            // Ensure container exists
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            // Generate a unique name so files don't overwrite
            string uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var blobClient = blobContainerClient.GetBlobClient(uniqueFileName);

            // Set the content type (image/jpeg, etc)
            var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };

            // Upload the file
            await blobClient.UploadAsync(fileStream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });

            // Return the URL
            return blobClient.Uri.ToString();
        }
    }
}