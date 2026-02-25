using Contracts.Dtos.Common;
using MediatR;

namespace PurchaseManagement.Application.ServiceMaster.Queries.GetAllServices
{
    public class GetAllServicesMasterQuery   : IRequest<ApiResponse<List<GetServiceMasterDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}