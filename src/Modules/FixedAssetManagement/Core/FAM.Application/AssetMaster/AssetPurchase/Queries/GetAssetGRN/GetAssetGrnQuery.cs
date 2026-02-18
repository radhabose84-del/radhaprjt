using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetGRN
{
    public class GetAssetGrnQuery : IRequest<List<GetAssetGrnDto>>
    {
        
         public string? OldUnitId { get; set; } 
         public string? SearchGrnNo { get; set; }
         public int AssetSourceId { get; set; }
    }
}