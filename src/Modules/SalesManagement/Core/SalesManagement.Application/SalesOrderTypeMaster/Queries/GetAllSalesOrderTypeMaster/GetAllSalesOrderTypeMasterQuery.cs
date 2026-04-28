using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesOrderTypeMaster.Dto;

namespace SalesManagement.Application.SalesOrderTypeMaster.Queries.GetAllSalesOrderTypeMaster
{
    public class GetAllSalesOrderTypeMasterQuery : IRequest<ApiResponseDTO<List<SalesOrderTypeMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
