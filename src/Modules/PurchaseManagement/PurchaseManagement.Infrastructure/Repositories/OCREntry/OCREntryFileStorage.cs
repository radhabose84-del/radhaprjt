using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Infrastructure.Repositories.OCREntry;

internal sealed class OCREntryFileStorage : IOCREntryFileStorage
{
    private const string BaseFolder = "Resources";
    private const string ModuleFolder = "Purchase";
    private const string ContainerFolder = "OCRDocuments";
    private const string TempPrefix = "TEMP_";

    private readonly IIPAddressService _ipService;
    private readonly ICompanyLookup _companyLookup;
    private readonly IUnitLookup _unitLookup;
    private readonly IMiscMasterQueryRepository _misc;
    private readonly ILogger<OCREntryFileStorage> _logger;

    public OCREntryFileStorage(
        IIPAddressService ipService,
        ICompanyLookup companyLookup,
        IUnitLookup unitLookup,
        IMiscMasterQueryRepository misc,
        ILogger<OCREntryFileStorage> logger)
    {
        _ipService = ipService;
        _companyLookup = companyLookup;
        _unitLookup = unitLookup;
        _misc = misc;
        _logger = logger;
    }

    public async Task<SavedOCRDocument> SaveAsync(IFormFile file, CancellationToken ct)
    {
        var targetDir = await ResolveUnitDirAsync();
        if (!Directory.Exists(targetDir))
            Directory.CreateDirectory(targetDir);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{TempPrefix}{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(targetDir, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        _logger.LogInformation("OCR document saved: {Original} -> {Path}", file.FileName, filePath);

        return new SavedOCRDocument(
            FileName: fileName,
            OriginalFileName: file.FileName,
            FileSize: file.Length,
            FileType: file.ContentType);
    }

    public async Task<string> RenameAsync(string currentFileName, string ocrNumber, CancellationToken ct)
    {
        var dir = await ResolveUnitDirAsync();
        var sourcePath = Path.Combine(dir, currentFileName);
        if (!File.Exists(sourcePath))
            throw new FileNotFoundException($"OCR document not found: {currentFileName}", sourcePath);

        var extension = Path.GetExtension(currentFileName);
        var newFileName = $"{SanitizeFileName(ocrNumber)}{extension}";
        var destPath = Path.Combine(dir, newFileName);

        File.Move(sourcePath, destPath, overwrite: true);

        _logger.LogInformation("OCR document renamed: {Old} -> {New}", currentFileName, newFileName);

        return newFileName;
    }

    public async Task<bool> DeleteAsync(string? fileName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        try
        {
            var filePath = await ResolvePhysicalPathAsync(fileName);
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("OCR document file deleted: {Path}", filePath);
                return true;
            }

            _logger.LogWarning("OCR document file not found at delete time: {Path}", filePath);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete OCR document file: {FileName}", fileName);
            return false;
        }
    }

    /// <summary>
    /// Resolves the local physical path for a delete request:
    ///  - Full public URL (starts with the OCRPath base URL from MiscMaster) → strip the base URL and
    ///    prepend the local "Resources" folder, e.g.
    ///    http://192.168.1.126/Resources/Purchase/OCRDocuments/BASML/Knitting/OCR_2025_0001.png
    ///    → Resources\Purchase\OCRDocuments\BASML\Knitting\OCR_2025_0001.png
    ///  - Bare file name → current token's company/unit folder.
    /// </summary>
    private async Task<string> ResolvePhysicalPathAsync(string fileNameOrUrl)
    {
        var baseUrl = await _misc.GetMiscTypeDescriptionAsync(MiscEnumEntity.ImagePath) ?? string.Empty;
        var normalizedInput = fileNameOrUrl.Replace('\\', '/');
        var trimmedBase = baseUrl.Replace('\\', '/').TrimEnd('/');

        if (trimmedBase.Length > 0 &&
            normalizedInput.StartsWith(trimmedBase, StringComparison.OrdinalIgnoreCase))
        {
            // {baseUrl}/Purchase/OCRDocuments/{Company}/{Unit}/{file} → Resources/Purchase/OCRDocuments/{Company}/{Unit}/{file}
            var relative = normalizedInput.Substring(trimmedBase.Length).TrimStart('/');
            return Path.Combine(BaseFolder, relative.Replace('/', Path.DirectorySeparatorChar));
        }

        // Bare file name → resolve company/unit from the token.
        var bareName = Path.GetFileName(normalizedInput);
        if (string.IsNullOrWhiteSpace(bareName))
            return string.Empty;

        var dir = await ResolveUnitDirAsync();
        return Path.Combine(dir, bareName);
    }

    public async Task<string> BuildPublicUrlAsync(string baseUrl, string fileName, CancellationToken ct)
    {
        var (companyName, unitName) = await ResolveCompanyUnitAsync();
        var prefix = (baseUrl ?? string.Empty).TrimEnd('/');

        // {baseUrl}/Purchase/OCRDocuments/{Company}/{Unit}/{fileName}
        return $"{prefix}/{ModuleFolder}/{ContainerFolder}/{companyName}/{unitName}/{fileName}";
    }

    // Unit-wise separate folder: Resources/Purchase/OCRDocuments/{Company}/{Unit}
    private async Task<string> ResolveUnitDirAsync()
    {
        var (companyName, unitName) = await ResolveCompanyUnitAsync();
        return Path.Combine(BaseFolder, ModuleFolder, ContainerFolder, companyName, unitName);
    }

    // Resolves sanitized company/unit folder names from the current token.
    private async Task<(string CompanyName, string UnitName)> ResolveCompanyUnitAsync()
    {
        var companyId = _ipService.GetCompanyId() ?? 0;
        var unitId = _ipService.GetUnitId() ?? 0;

        var companies = await _companyLookup.GetAllCompanyAsync();
        var companyName = SanitizeFileName(
            companies.FirstOrDefault(c => c.CompanyId == companyId)?.CompanyName ?? "Default");

        var units = await _unitLookup.GetAllUnitAsync();
        var unitName = SanitizeFileName(
            units.FirstOrDefault(u => u.UnitId == unitId)?.UnitName ?? "Default");

        return (companyName, unitName);
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var clean = string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c)).Trim();
        return string.IsNullOrWhiteSpace(clean) ? "Default" : clean;
    }
}
