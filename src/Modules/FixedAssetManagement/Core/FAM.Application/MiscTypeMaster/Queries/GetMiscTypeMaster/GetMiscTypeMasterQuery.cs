using MediatR;
using Contracts.Common;

namespace FAM.Application.MiscTypeMaster.Queries.GetMiscTypeMaster
{
    public class GetMiscTypeMasterQuery  : IRequest<ApiResponseDTO<List<GetMiscTypeMasterDto>>>
    {


         public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
        
    }
}