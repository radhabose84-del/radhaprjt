using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.Mappings;
using FAM.Domain.Entities.AssetMaster;

namespace FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty
{
    public class AssetWarrantyImage  : IMapFrom<AssetWarranties>
    {
          public string? AssetImage { get; set; }
        public string? AssetImageBase64 { get; set; } 
    }
}