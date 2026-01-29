using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecificationBasedMachineNo
{
    public class AssetSpecBasedOnMachineNoDto
    {
        public int AssetId { get; set; }
        public string? SpecificationName { get; set; }
        public string? SpecificationValue { get; set; }
        public DateTimeOffset CapitalizationDate { get; set; }

    }
}