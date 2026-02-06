using System;

namespace Contracts.Dtos.Lookups.FixedAssetManagement
{
    public class AssetSpecificationLookupDto
    {
        public int AssetId { get; set; }
        public string? SpecificationName { get; set; }
        public string? SpecificationValue { get; set; }
        public DateTimeOffset CapitalizationDate { get; set; }
    }
}
