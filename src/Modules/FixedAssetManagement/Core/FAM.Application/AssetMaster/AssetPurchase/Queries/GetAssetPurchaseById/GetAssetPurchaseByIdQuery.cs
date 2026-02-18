using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetPurchase;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetPurchaseById
{
    public class GetAssetPurchaseByIdQuery : IRequest<AssetPurchaseDetailsDto>
    {
         public int Id { get; set; }
    }
}