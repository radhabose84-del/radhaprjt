namespace FinanceManagement.Application.Common.Interfaces.ITaxCode
{
    // Consolidated command repository for the Tax Code feature
    // (TaxCodeMaster + TaxCodeRateVersion + TaxAccountLinkage + GstrSectionMapping).
    public interface ITaxCodeCommandRepository
    {
        // --- Tax Code Master (US-GL02-05A) ---
        Task<int> CreateTaxCodeAsync(Domain.Entities.TaxCodeMaster entity);
        Task<int> UpdateTaxCodeAsync(Domain.Entities.TaxCodeMaster entity);
        Task<bool> SoftDeleteTaxCodeAsync(int id, CancellationToken ct);
        Task<int> CreateRateVersionAsync(Domain.Entities.TaxCodeRateVersion entity);

        // --- Tax Account Linkage (US-GL02-05B) ---
        Task<int> CreateLinkageAsync(Domain.Entities.TaxAccountLinkage entity);
        Task<bool> ActivateLinkageAsync(int id, CancellationToken ct);
        Task<bool> SoftDeleteLinkageAsync(int id, CancellationToken ct);

        // --- GSTR Section Mapping (US-GL02-05B AC3) ---
        Task<int> CreateGstrMappingAsync(Domain.Entities.GstrSectionMapping entity);
        Task<int> UpdateGstrMappingAsync(Domain.Entities.GstrSectionMapping entity);
        Task<bool> SoftDeleteGstrMappingAsync(int id, CancellationToken ct);
    }
}
