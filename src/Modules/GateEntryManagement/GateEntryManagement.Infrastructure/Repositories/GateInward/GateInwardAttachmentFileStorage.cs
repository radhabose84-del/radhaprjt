using GateEntryManagement.Application.Common.Interfaces.IGateInward;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GateEntryManagement.Infrastructure.Repositories.GateInward
{
    internal sealed class GateInwardAttachmentFileStorage : IGateInwardAttachmentFileStorage
    {
        private const string ResourcesRoot = "Resources";
        private const string StagingFolder = "Gate/GateInward/_staging";

        private readonly ILogger<GateInwardAttachmentFileStorage> _logger;

        public GateInwardAttachmentFileStorage(ILogger<GateInwardAttachmentFileStorage> logger)
        {
            _logger = logger;
        }

        public async Task<StagedGateInwardAttachment> SaveToStagingAsync(IFormFile file, CancellationToken ct)
        {
            var stagingDir = Path.Combine(Directory.GetCurrentDirectory(), ResourcesRoot, StagingFolder);
            Directory.CreateDirectory(stagingDir);

            var extension = Path.GetExtension(file.FileName);
            var stagedFileName = $"TEMP_{Guid.NewGuid()}{extension}";
            var stagedPath = Path.Combine(stagingDir, stagedFileName);

            await using (var stream = new FileStream(stagedPath, FileMode.Create))
            {
                await file.CopyToAsync(stream, ct);
            }

            _logger.LogInformation("GateInward attachment staged: {Original} -> {Staged}", file.FileName, stagedFileName);

            return new StagedGateInwardAttachment(FileName: stagedFileName);
        }

        public Task<string> MoveStagedToPermanentAsync(
            string stagedFileName, string subFolder, string desiredBaseName, CancellationToken ct)
        {
            var stagingPath = Path.Combine(Directory.GetCurrentDirectory(), ResourcesRoot, StagingFolder, stagedFileName);
            if (!File.Exists(stagingPath))
                throw new FileNotFoundException($"Staged GateInward attachment not found: {stagedFileName}", stagingPath);

            if (string.IsNullOrWhiteSpace(subFolder))
                throw new InvalidOperationException("GateEntryImage misc folder is not configured in Gate.MiscTypeMaster.");

            var permanentDir = Path.Combine(Directory.GetCurrentDirectory(), ResourcesRoot, subFolder);
            Directory.CreateDirectory(permanentDir);

            var extension = Path.GetExtension(stagedFileName);
            var permanentFileName = $"{Sanitize(desiredBaseName)}{extension}";
            var permanentPath = Path.Combine(permanentDir, permanentFileName);

            // overwrite: single attachment per gate entry — re-upload replaces the old file.
            File.Move(stagingPath, permanentPath, overwrite: true);

            // Relative to Resources root — used to compose preview URL and for delete.
            var relativePath = $"{subFolder}/{permanentFileName}";
            _logger.LogInformation("GateInward attachment moved to permanent: {Staged} -> {Relative}", stagedFileName, relativePath);

            return Task.FromResult(relativePath);
        }

        // Replaces filesystem-illegal characters (incl. / and \ from GateEntryNo) with '-'.
        private static string Sanitize(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Guid.NewGuid().ToString("N");

            var invalid = Path.GetInvalidFileNameChars();
            var cleaned = new string(name.Select(c => invalid.Contains(c) ? '-' : c).ToArray())
                .Replace('/', '-')
                .Replace('\\', '-');

            while (cleaned.Contains("--"))
                cleaned = cleaned.Replace("--", "-");

            cleaned = cleaned.Trim('-', ' ', '.');
            return string.IsNullOrWhiteSpace(cleaned) ? Guid.NewGuid().ToString("N") : cleaned;
        }

        public Task DeleteAsync(string? relativePath, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return Task.CompletedTask;

            try
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), ResourcesRoot, relativePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("GateInward attachment file deleted: {Path}", relativePath);
                }
                else
                {
                    _logger.LogWarning("GateInward attachment file not found at delete time: {Path}", relativePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete GateInward attachment file: {Path}", relativePath);
            }

            return Task.CompletedTask;
        }
    }
}
