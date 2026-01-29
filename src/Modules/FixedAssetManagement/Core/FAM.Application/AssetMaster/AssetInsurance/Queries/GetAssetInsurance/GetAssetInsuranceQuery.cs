using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetInsurance.Queries.GetAssetInsurance
{
    public class GetAssetInsuranceQuery  : IRequest<ApiResponseDTO<List<GetAssetInsuranceDto>>>
    {
         public int PageNumber { get; set; }
        public int PageSize { get; set; } 
        public string? SearchTerm { get; set; }
    }


}