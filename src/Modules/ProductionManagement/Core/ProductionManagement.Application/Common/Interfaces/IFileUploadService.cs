namespace ProductionManagement.Application.Common.Interfaces
{
    public interface IFileUploadService
    {
        Task<string> UploadFileAsync(string base64File, string fileName, string folderPath);
        Task<bool> DeleteFileAsync(string filePath);
    }
}
