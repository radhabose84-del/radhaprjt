using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.MachineMaster.Queries.GetAssetSpecificationById
{
    public class AssetSpecificationByAssetIdDto
    {
        public int AssetId { get; set; }
        public string? SpecificationName { get; set; }
        public string? SpecificationValue { get; set; }
        public DateTimeOffset CapitalizationDate { get; set; }
    }
}