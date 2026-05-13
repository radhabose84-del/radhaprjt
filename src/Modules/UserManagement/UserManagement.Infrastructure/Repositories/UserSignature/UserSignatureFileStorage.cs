using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IUserSignature;

namespace UserManagement.Infrastructure.Repositories.UserSignature
{
    internal sealed class UserSignatureFileStorage : IUserSignatureFileStorage
    {
        private const string BaseFolder = "Resources";
        private const string ModuleFolder = "UserManagement";
        private const string ContainerFolder = "UserSignatures";

        private readonly ILogger<UserSignatureFileStorage> _logger;

        public UserSignatureFileStorage(ILogger<UserSignatureFileStorage> logger)
        {
            _logger = logger;
        }

        public async Task<SavedUserSignatureFile> SaveAsync(IFormFile file, string userName, int userId, CancellationToken cancellationToken)
        {
            var sanitized = SanitizeUserName(userName);
            var extension = (Path.GetExtension(file.FileName) ?? string.Empty).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension))
            {
                extension = file.ContentType?.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase) == true
                    ? ".jpg"
                    : ".png";
            }

            var generatedFileName = $"{sanitized}-{userId}{extension}";

            var directory = Path.Combine(BaseFolder, ModuleFolder, ContainerFolder);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Delete any existing file for this user (handles png ↔ jpg extension change)
            DeleteAnyExistingFor(sanitized, userId);

            var fullPath = Path.Combine(directory, generatedFileName);
            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            _logger.LogInformation("UserSignature saved: {FilePath}", fullPath);

            return new SavedUserSignatureFile(
                FileName: generatedFileName,
                OriginalFileName: file.FileName,
                FilePath: fullPath,
                FileType: file.ContentType ?? "application/octet-stream",
                FileSize: file.Length);
        }

        public Task DeleteAsync(string? filePath, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return Task.CompletedTask;
            }

            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("UserSignature file deleted: {FilePath}", filePath);
                }
                else
                {
                    _logger.LogWarning("UserSignature file not found at delete time: {FilePath}", filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete UserSignature file: {FilePath}", filePath);
            }

            return Task.CompletedTask;
        }

        public async Task<string?> ReadAsBase64Async(string? filePath, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return null;
            }

            var bytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
            return Convert.ToBase64String(bytes);
        }

        private void DeleteAnyExistingFor(string sanitizedUserName, int userId)
        {
            var directory = Path.Combine(BaseFolder, ModuleFolder, ContainerFolder);
            if (!Directory.Exists(directory))
            {
                return;
            }

            foreach (var existing in Directory.EnumerateFiles(directory, $"{sanitizedUserName}-{userId}.*"))
            {
                try
                {
                    File.Delete(existing);
                    _logger.LogInformation("UserSignature removed prior file: {FilePath}", existing);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to remove prior UserSignature file: {FilePath}", existing);
                }
            }
        }

        private static string SanitizeUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return "user";
            }

            var firstToken = userName.Split(new[] { ' ', '\t', '_', '-', '.' }, StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault() ?? "user";

            var cleaned = new string(firstToken
                .ToLowerInvariant()
                .Where(char.IsLetterOrDigit)
                .ToArray());

            return string.IsNullOrEmpty(cleaned) ? "user" : cleaned;
        }
    }
}
