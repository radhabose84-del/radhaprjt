using PurchaseManagement.Application.BarcodeSeries.Dto;

namespace PurchaseManagement.Application.Common.Interfaces.IBarcodeSeries
{
    public interface IBarcodeSeriesQueryRepository
    {
        Task<(List<BarcodeSeriesDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<BarcodeSeriesDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<BarcodeSeriesLookupDto>> AutocompleteAsync(string term, CancellationToken ct);

        // True when the supplied range overlaps any existing non-deleted series (R1/R3). Excludes self on update.
        Task<bool> RangeOverlapsAsync(long start, long end, int? id = null);

        // True when the prefix Id is an active, non-deleted MiscMaster value of type BarcodePrefix.
        Task<bool> IsValidPrefixAsync(int prefixId);

        // True when the series exists (mirrors module NotFoundAsync convention: returns existence).
        Task<bool> NotFoundAsync(int id);

        // True when the series already has allocated barcodes (blocks edit/delete).
        Task<bool> IsAllocatedAsync(int id);

        // Highest BarcodeEndNumber across non-deleted series (drives next-start suggestion).
        Task<long?> GetMaxEndNumberAsync();

        // Display-only peek of the next series number for the supplied generation date.
        Task<string> PeekNextSeriesNumberAsync(DateTimeOffset generationDate);
    }
}
