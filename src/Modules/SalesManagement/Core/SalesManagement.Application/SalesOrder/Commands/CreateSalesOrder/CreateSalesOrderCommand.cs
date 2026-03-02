using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesOrder.Dto;

namespace SalesManagement.Application.SalesOrder.Commands.CreateSalesOrder
{
    public class CreateSalesOrderCommand : IRequest<ApiResponseDTO<int>>
    {
        public CreateSalesOrderDto? SalesOrderDetails { get; set; }
    }
}
