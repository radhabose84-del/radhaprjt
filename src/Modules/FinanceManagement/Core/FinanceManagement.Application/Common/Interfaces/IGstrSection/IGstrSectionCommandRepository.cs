namespace FinanceManagement.Application.Common.Interfaces.IGstrSection
{
    // Consolidated command repository for the GSTR section feature
    // (GstrSectionMaster + GstrSectionAccountLinkage).
    public interface IGstrSectionCommandRepository
    {
        // --- Section Master ---
        Task<int> CreateSectionAsync(Domain.Entities.GstrSectionMaster entity);
        Task<int> UpdateSectionAsync(Domain.Entities.GstrSectionMaster entity);
        Task<bool> DeleteSectionAsync(int id, CancellationToken ct);

        // --- Section Account Linkage ---
        Task<int> CreateLinkageAsync(Domain.Entities.GstrSectionAccountLinkage entity);
        Task<int> UpdateLinkageAsync(Domain.Entities.GstrSectionAccountLinkage entity);
        Task<bool> DeleteLinkageAsync(int id, CancellationToken ct);
    }
}
