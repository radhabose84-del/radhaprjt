using Contracts.Dtos.Lookups.Purchase;

namespace Contracts.Interfaces.Lookups.Purchase;

public interface IVendorRatingLookup
{
    Task<VendorRatingLookupDto?> GetLatestRatingByVendorIdAsync(int vendorId, CancellationToken ct = default);
}
