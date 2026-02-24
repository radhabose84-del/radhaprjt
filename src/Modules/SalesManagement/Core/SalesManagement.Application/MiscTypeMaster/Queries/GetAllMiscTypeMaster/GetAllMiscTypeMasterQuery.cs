#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.MiscTypeMaster.Dto;

namespace SalesManagement.Application.MiscTypeMaster.Queries.GetAllMiscTypeMaster
{
    public class GetAllMiscTypeMasterQuery : IRequest<ApiResponseDTO<List<MiscTypeMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SearchTerm { get; set; }
    }
}
