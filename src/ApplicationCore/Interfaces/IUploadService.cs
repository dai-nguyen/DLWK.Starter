using ApplicationCore.Requests;

namespace ApplicationCore.Interfaces
{
    public interface IFileService
    {
        Task<string> UploadAsync(UploadRequest request);
        Task DeleteAsync(string dbPath);
    }
}
