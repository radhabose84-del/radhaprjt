using PurchaseManagement.Application.BarcodeAllocation.Dto;

namespace PurchaseManagement.Application.Common.Interfaces.IBarcodeAllocation
{
    public interface IBarcodeAllocationQueryRepository
    {
        Task<(List<BarcodeAllocationDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<BarcodeAllocationDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<BarcodeAllocationLookupDto>> AutocompleteAsync(string term, CancellationToken ct);

        // Passing-person dropdown — external HR, scoped to the current user's division.
        Task<IReadOnlyList<BarcodeAllocationEmployeeDto>> GetEmployeesAsync(string? term, CancellationToken ct);

        // Series dropdown — series that still have un-allocated range. When seriesId is supplied, returns just
        // that one series (Edit mode), even if it is fully allocated.
        Task<IReadOnlyList<BarcodeAllocationSeriesDto>> GetAvailableSeriesAsync(string? term, int? seriesId = null);

        // R1 — true when the range overlaps another allocation of the SAME series. Excludes self on update.
        Task<bool> RangeOverlapsInSeriesAsync(int barcodeSeriesId, long from, long to, int? id = null);

        // R2 — true when [from,to] sits inside the selected series' Start..End.
        Task<bool> IsWithinSeriesRangeAsync(int barcodeSeriesId, long from, long to);

        // True when the series exists, is active and not deleted.
        Task<bool> SeriesExistsAsync(int barcodeSeriesId);

        // True when the allocation exists (module NotFoundAsync convention: returns existence).
        Task<bool> NotFoundAsync(int id);

        // True when the allocation already has used (inwarded) barcodes (blocks edit/delete).
        Task<bool> IsUsedAsync(int id);

        // Highest already-allocated BarcodeTo within a series (drives next-from suggestion).
        Task<long?> GetMaxAllocatedToForSeriesAsync(int barcodeSeriesId);

        // Display-only peek of the next allocation number for the supplied date.
        Task<string> PeekNextAllocationNumberAsync(DateTimeOffset allocationDate);
    }
}
