using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.Mappings;
using FAM.Domain.Entities;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral
{
    public class AssetMasterImageDto : IMapFrom<AssetMasterGenerals>
    {
        public string? AssetImage { get; set; }
        public string? AssetImageBase64 { get; set; } 

    }
}