namespace PurchaseManagement.Application.Common.Interfaces.IBarcodeSeries
{
    public interface IBarcodeSeriesCommandRepository
    {
        // Generates the immutable BarcodeSeriesNumber and the default status, then persists. Returns the new Id.
        Task<int> CreateAsync(PurchaseManagement.Domain.Entities.BarcodeSeries entity);

        // Updates the mutable fields only (BarcodeSeriesNumber is immutable). Returns the affected Id.
        Task<int> UpdateAsync(PurchaseManagement.Domain.Entities.BarcodeSeries entity);

        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
