using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetGRNItem
{
    public class GetAssetGrnItemQuery : IRequest<List<AssetGrnItemDto>>
    {
        public string? OldUnitId { get; set; } 
         public int AssetSourceId { get; set; }
         public int GrnNo { get; set; } 
    }
}