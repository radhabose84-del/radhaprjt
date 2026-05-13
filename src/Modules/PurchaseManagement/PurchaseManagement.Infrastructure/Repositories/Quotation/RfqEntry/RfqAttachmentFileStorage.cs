using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;

namespace PurchaseManagement.Infrastructure.Repositories.Quotation.RfqEntry;

internal sealed class RfqAttachmentFileStorage : IRfqAttachmentFileStorage
{
    private const string BaseFolder = "Resources";
    private const string ModuleFolder = "Purchase";
    private const string ContainerFolder = "RfqAttachments";
    private const string StagingFolder = "_staging";

    private readonly IIPAddressService _ipService;
    private readonly ICompanyLookup _companyLookup;
    private readonly IUnitLookup _unitLookup;
    private readonly ILogger<RfqAttachmentFileStorage> _logger;

    public RfqAttachmentFileStorage(
        IIPAddressService ipService,
        ICompanyLookup companyLookup,
        IUnitLookup unitLookup,
        ILogger<RfqAttachmentFileStorage> logger)
    {
        _ipService = ipService;
        _companyLookup = companyLookup;
        _unitLookup = unitLookup;
        _logger = logger;
    }

    public async Task<StagedRfqAttachment> SaveToStagingAsync(IFormFile file, CancellationToken ct)
    {
        var stagingDir = Path.Combine(BaseFolder, ModuleFolder, ContainerFolder, StagingFolder);
        if (!Directory.Exists(stagingDir))
            Directory.CreateDirectory(stagingDir);

        var extension = Path.GetExtension(file.FileName);
        var stagedFileName = $"TEMP_{Guid.NewGuid()}{extension}";
        var stagedPath = Path.Combine(stagingDir, stagedFileName);

        await using (var stream = new FileStream(stagedPath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        _logger.LogInformation("RFQ attachment staged: {Original} -> {Staged}", file.FileName, stagedFileName);

        return new StagedRfqAttachment(
            FileName: stagedFileName,
            OriginalFileName: file.FileName,
            FileSize: file.Length,
            FileType: file.ContentType);
    }

    public async Task<string> MoveStagedToPermanentAsync(string stagedFileName, CancellationToken ct)
    {
        var stagingPath = Path.Combine(BaseFolder, ModuleFolder, ContainerFolder, StagingFolder, stagedFileName);
        if (!File.Exists(stagingPath))
            throw new FileNotFoundException($"Staged RFQ attachment not found: {stagedFileName}", stagingPath);

        var companyId = _ipService.GetCompanyId() ?? 0;
        var unitId = _ipService.GetUnitId() ?? 0;

        var companies = await _companyLookup.GetAllCompanyAsync();
        var companyName = SanitizeFolderName(
            companies.FirstOrDefault(c => c.CompanyId == companyId)?.CompanyName ?? "Default");

        var units = await _unitLookup.GetAllUnitAsync();
        var unitName = SanitizeFolderName(
            units.FirstOrDefault(u => u.UnitId == unitId)?.UnitName ?? "Default");

        var permanentDir = Path.Combine(BaseFolder, ModuleFolder, ContainerFolder, companyName, unitName);
        if (!Directory.Exists(permanentDir))
            Directory.CreateDirectory(permanentDir);

        var extension = Path.GetExtension(stagedFileName);
        var permanentFileName = $"{Guid.NewGuid()}{extension}";
        var permanentPath = Path.Combine(permanentDir, permanentFileName);

        File.Move(stagingPath, permanentPath);

        _logger.LogInformation("RFQ attachment moved to permanent: {Staged} -> {Permanent}", stagedFileName, permanentPath);

        return permanentPath;
    }

    public Task DeleteAsync(string? filePath, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return Task.CompletedTask;

        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("RFQ attachment file deleted: {Path}", filePath);
            }
            else
            {
                _logger.LogWarning("RFQ attachment file not found at delete time: {Path}", filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete RFQ attachment file: {Path}", filePath);
        }

        return Task.CompletedTask;
    }

    private static string SanitizeFolderName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var clean = string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c)).Trim();
        return string.IsNullOrWhiteSpace(clean) ? "Default" : clean;
    }
}
