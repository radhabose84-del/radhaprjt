using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesOrder.Dto;

namespace SalesManagement.Application.SalesOrder.Commands.CreateSalesOrderAmendment
{
    public class CreateSalesOrderAmendmentCommand : IRequest<ApiResponseDTO<int>>
    {
        public int SalesOrderHeaderId { get; set; }
        public string? Reason { get; set; }
        public List<CreateSalesOrderAmendmentDetailDto>? AmendmentDetails { get; set; }
    }
}
