using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetMaster.AssetAdditionalCost.Queries.GetAssetAdditionalCost;
using Contracts.Common;
using FAM.Application.Common.Mappings.AssetMaster;
using MediatR;

namespace FAM.Application.AssetMaster.AssetAdditionalCost.Queries.GetAssetAdditionalCostById
{
    public class GetAssetAdditionalCostByIdQuery : IRequest<AssetAdditionalCostDto>
    {
        public int Id { get; set; }
    }
}