using FinanceManagement.Application.TaxCode.Dto;

namespace FinanceManagement.Application.Common.Interfaces.IGstrSection
{
    // Consolidated query repository for the GSTR section feature.
    public interface IGstrSectionQueryRepository
    {
        // --- Section Master ---
        Task<(List<GstrSectionMasterDto>, int)> GetAllSectionsAsync(int pageNumber, int pageSize, string? searchTerm, int? companyId, int? reportTypeId);
        Task<GstrSectionMasterDto?> GetSectionByIdAsync(int id);
        Task<IReadOnlyList<GstrSectionMasterLookupDto>> SectionAutocompleteAsync(string term, int? reportTypeId, int? companyId, CancellationToken ct);
        Task<bool> SectionAlreadyExistsAsync(int companyId, int reportTypeId, string sectionCode, int? id = null);
        Task<bool> SectionNotFoundAsync(int id);
        Task<bool> SectionExistsAsync(int id);
        Task<bool> SectionHasLinkagesAsync(int id);           // blocks delete when account mappings exist
        Task<bool> ReportTypeExistsAsync(int id);             // FK -> MiscMaster (GSTR_REPORT)

        // --- Section Account Linkage ---
        Task<(List<GstrSectionAccountLinkageDto>, int)> GetAllLinkagesAsync(int pageNumber, int pageSize, string? searchTerm, int? companyId);
        Task<GstrSectionAccountLinkageDto?> GetLinkageByIdAsync(int id);
        Task<bool> LinkageNotFoundAsync(int id);
    }
}
