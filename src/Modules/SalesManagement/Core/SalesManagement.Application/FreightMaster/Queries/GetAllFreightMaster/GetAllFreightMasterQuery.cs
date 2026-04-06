using Contracts.Common;
using MediatR;
using SalesManagement.Application.FreightMaster.Dto;

namespace SalesManagement.Application.FreightMaster.Queries.GetAllFreightMaster
{
    public class GetAllFreightMasterQuery : IRequest<ApiResponseDTO<List<FreightMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
