using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using FAM.Domain.Entities.AssetPurchase;
using MediatR;

namespace FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetGrnDetails
{
    public class GetAssetDetailsQuery : IRequest<List<AssetGrnDetails>>
    {
        public string?  OldUnitId { get; set; } 
        public int AssetSourceId { get; set; }
        public int GrnNo { get; set; } 
        public int GrnSerialNo { get; set; }
    }
}