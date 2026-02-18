using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetMaster.AssetAdditionalCost.Queries.GetAssetAdditionalCost
{
    public class GetAssetAdditionalCostQuery : IRequest<ApiResponseDTO<List<AssetAdditionalCostDto>>>
    {

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}