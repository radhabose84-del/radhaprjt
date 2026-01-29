using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetLocation.Queries.GetAssetLocation
{
    public class GetAssetLocationQuery  : IRequest<ApiResponseDTO<List<AssetLocationDto>>>
    {
         public int PageNumber { get; set; }
        public int PageSize { get; set; } 
        public string? SearchTerm { get; set; }
    }
}