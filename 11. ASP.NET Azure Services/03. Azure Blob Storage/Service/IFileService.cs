using BlobStorageApplication.Model;

namespace BlobStorageApplication.Service
{
    public interface IFileService
    {
        Task Upload(FileModel fileModel);
        Task<Stream> Download(string name);
    }
}
