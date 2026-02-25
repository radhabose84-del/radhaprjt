using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrderById
{
    public class GetWorkOrderByIdQuery  : IRequest<ApiResponseDTO<GetWorkOrderByIdDto>>
    {
        public int Id { get; set; }
    }
}