namespace FinanceManagement.Application.Common.Interfaces.ITaxCode
{
    // Consolidated command repository for the Tax Code feature
    // (TaxCodeMaster + TaxCodeRateVersion + TaxAccountLinkage).
    public interface ITaxCodeCommandRepository
    {
        // --- Tax Code Master (US-GL02-05A) — no delete; "remove" = Update IsActive=0 ---
        Task<int> CreateTaxCodeAsync(Domain.Entities.TaxCodeMaster entity);
        Task<int> UpdateTaxCodeAsync(Domain.Entities.TaxCodeMaster entity);
        Task<int> CreateRateVersionAsync(Domain.Entities.TaxCodeRateVersion entity);

        // --- Tax Account Linkage (US-GL02-05B) — no delete; updates insert a new row ---
        Task<int> CreateLinkageAsync(Domain.Entities.TaxAccountLinkage entity);
        Task<bool> ActivateLinkageAsync(int id, int approvedStatusId, CancellationToken ct);   // approve: Active + APPROVED, close prior
        Task<bool> RejectLinkageAsync(int id, int rejectedStatusId, CancellationToken ct);     // reject: StatusId=REJECTED, stays Inactive
    }
}
