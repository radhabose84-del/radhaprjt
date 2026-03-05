using Contracts.Common;
using MediatR;
using SalesManagement.Application.StoTypeMaster.Dto;

namespace SalesManagement.Application.StoTypeMaster.Queries.GetAllStoTypeMaster
{
    public class GetAllStoTypeMasterQuery : IRequest<ApiResponseDTO<List<StoTypeMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
