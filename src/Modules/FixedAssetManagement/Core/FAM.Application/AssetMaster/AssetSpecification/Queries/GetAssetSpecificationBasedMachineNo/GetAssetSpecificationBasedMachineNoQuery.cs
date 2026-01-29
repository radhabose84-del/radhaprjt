using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecificationBasedMachineNo
{
    public class GetAssetSpecificationBasedMachineNoQuery : IRequest<ApiResponseDTO<List<AssetSpecBasedOnMachineNoDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
            
}