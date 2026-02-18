using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Maintenance
{
    public class AssetSpecificationDto
    {
        public int AssetId { get; set; }
        public string SpecificationName { get; set; } = default!;
        public string SpecificationValue { get; set; } = default!;
        public DateTimeOffset CapitalizationDate { get; set; }
    }
}