using Azure.Storage.Blobs;
using BlobStorageApplication.Model;
using Microsoft.Extensions.Azure;

namespace BlobStorageApplication.Service
{
    public class FileService : IFileService
    {
        public readonly BlobServiceClient blobServiceClient;

        public FileService(BlobServiceClient blobServiceClient)
        {
            this.blobServiceClient = blobServiceClient;
        }

        public async Task Upload(FileModel fileModel)
        {
            var container = this.blobServiceClient.GetBlobContainerClient("sacontainer46310114");

            var blobImage = container.GetBlobClient(fileModel.formFile.FileName);

            await blobImage.UploadAsync(fileModel.formFile.OpenReadStream());
        }

        public async Task<Stream> Download(string name)
        {
            var container = this.blobServiceClient.GetBlobContainerClient("sacontainer46310114");

            var blobImage = container.GetBlobClient(name);

            var downloadContent = await blobImage.DownloadAsync();

            return downloadContent.Value.Content;
        }
    }
}
