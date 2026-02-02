namespace PurchaseManagement.Application.Common.Interfaces.IPoMethodLookup
{
    public interface IPoMethodLookup
    {
        Task<int> GetLocalIdAsync(CancellationToken ct);
        Task<int> GetImportIdAsync(CancellationToken ct);
        Task<bool> IsLocalAsync(int poMethodId, CancellationToken ct);
        Task<bool> IsImportAsync(int poMethodId, CancellationToken ct);
        Task<bool> IsValidAsync(int poMethodId, CancellationToken ct);
    }
}
