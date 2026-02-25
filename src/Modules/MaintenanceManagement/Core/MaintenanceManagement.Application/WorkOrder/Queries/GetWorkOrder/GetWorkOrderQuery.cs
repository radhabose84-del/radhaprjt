using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrder
{
    public class GetWorkOrderQuery   : IRequest<ApiResponseDTO<List<Dictionary<string, List<GetWorkOrderDto>>>>>
    {
        public DateTimeOffset? fromDate {get; set;}
        public DateTimeOffset? toDate {get; set;}
        public int? requestTypeId {get; set;}
        public int? departmentId { get; set; } 
        public int? machineId { get; set; } 
    }
}