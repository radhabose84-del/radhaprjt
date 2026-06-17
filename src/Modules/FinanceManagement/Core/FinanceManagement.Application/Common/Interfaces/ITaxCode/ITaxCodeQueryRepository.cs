using FinanceManagement.Application.TaxCode.Dto;

namespace FinanceManagement.Application.Common.Interfaces.ITaxCode
{
    // Consolidated query repository for the Tax Code feature.
    public interface ITaxCodeQueryRepository
    {
        // --- Tax Code Master ---
        Task<(List<TaxCodeMasterDto>, int)> GetAllTaxCodesAsync(int pageNumber, int pageSize, string? searchTerm, int? companyId, string? taxType);
        Task<TaxCodeMasterDto?> GetTaxCodeByIdAsync(int id);
        Task<(List<TaxCodeGlMappingSummaryDto>, int)> GetTaxCodeGlMappingSummaryAsync(int pageNumber, int pageSize, string? searchTerm, int? companyId, string? taxType);
        Task<IReadOnlyList<TaxCodeMasterLookupDto>> TaxCodeAutocompleteAsync(string term, int? companyId, string? taxType, CancellationToken ct);
        Task<List<TaxCodeRateVersionDto>> GetRateVersionsAsync(int taxCodeId);
        Task<bool> TaxCodeAlreadyExistsAsync(string taxCode, int companyId, int? id = null);
        Task<bool> TaxCodeNotFoundAsync(int id);
        Task<bool> TaxCodeExistsAsync(int id);

        // MiscMaster lookups for TaxType / TaxComponent / Direction
        Task<string?> GetMiscCodeAsync(int miscMasterId); // normalised code for business-rule branching
        Task<bool> TaxTypeExistsAsync(int id);
        Task<bool> TaxComponentExistsAsync(int id);
        Task<bool> DirectionExistsAsync(int id);

        // --- Tax Account Linkage ---
        Task<(List<TaxAccountLinkageDto>, int)> GetAllLinkagesAsync(int pageNumber, int pageSize, string? searchTerm, int? companyId, int? statusId);
        Task<(List<PendingTaxAccountLinkageDto>, int)> GetPendingLinkagesAsync(int pageNumber, int pageSize, string? searchTerm, int? companyId);
        Task<TaxAccountLinkageDto?> GetLinkageByIdAsync(int id);
        Task<TaxAccountLinkageDto?> GetLinkageByAccountAsync(int glAccountId);
        Task<bool> GlAccountExistsAsync(int glAccountId);
        Task<bool> ControlAccountExistsAsync(int id);     // FK -> MiscMaster (control account type)
        Task<bool> LinkageNotFoundAsync(int id);
        Task<bool> LinkageHasGlMappingAsync(int id);      // AC2-B activation guard

        // Resolve a MiscMaster value id by misc-type code + value code (separator-insensitive on type).
        Task<int?> GetMiscIdAsync(string miscTypeCode, string valueCode);
    }
}
