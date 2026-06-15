using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesOrder.Dto;

namespace SalesManagement.Application.SalesOrder.Commands.CreateSalesOrder
{
    public class CreateSalesOrderCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public CreateSalesOrderDto? SalesOrderDetails { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
