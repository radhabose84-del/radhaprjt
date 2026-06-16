using FinanceManagement.Application.TaxCode.Dto;

namespace FinanceManagement.Application.Common.Interfaces.ITaxCode
{
    // Consolidated query repository for the Tax Code feature.
    public interface ITaxCodeQueryRepository
    {
        // --- Tax Code Master ---
        Task<(List<TaxCodeMasterDto>, int)> GetAllTaxCodesAsync(int pageNumber, int pageSize, string? searchTerm, int? companyId, string? taxType);
        Task<TaxCodeMasterDto?> GetTaxCodeByIdAsync(int id);
        Task<IReadOnlyList<TaxCodeMasterLookupDto>> TaxCodeAutocompleteAsync(string term, int? companyId, string? taxType, CancellationToken ct);
        Task<List<TaxCodeRateVersionDto>> GetRateVersionsAsync(int taxCodeId);
        Task<TaxCodeMasterDto?> GetEffectiveAsync(string code, int? companyId, DateOnly asOf);
        Task<bool> TaxCodeAlreadyExistsAsync(string taxCode, int companyId, int? id = null);
        Task<bool> TaxCodeNotFoundAsync(int id);
        Task<bool> TaxCodeExistsAsync(int id);
        Task<bool> TaxCodeLinkedAsync(int id);            // blocks delete when linked to GL accounts (AC5-A)

        // --- Tax Account Linkage ---
        Task<(List<TaxAccountLinkageDto>, int)> GetAllLinkagesAsync(int pageNumber, int pageSize, string? searchTerm, int? companyId);
        Task<TaxAccountLinkageDto?> GetLinkageByIdAsync(int id);
        Task<TaxAccountLinkageDto?> GetLinkageByAccountAsync(int glAccountId);
        Task<List<TaxAccountLinkageDto>> GetLinkageChangeAuditAsync(string? status, int? companyId);
        Task<bool> GlAccountExistsAsync(int glAccountId);
        Task<bool> LinkageNotFoundAsync(int id);
        Task<bool> LinkageHasGlMappingAsync(int id);      // AC2-B activation guard

        // --- GSTR Section Mapping ---
        Task<(List<GstrSectionMappingDto>, int)> GetAllGstrMappingsAsync(int pageNumber, int pageSize, string? searchTerm, int? companyId);
        Task<GstrSectionMappingDto?> GetGstrMappingByIdAsync(int id);
        Task<bool> GstrMappingAlreadyExistsAsync(int companyId, string gstrType, string sectionCode, int? id = null);
        Task<bool> GstrMappingNotFoundAsync(int id);
    }
}
