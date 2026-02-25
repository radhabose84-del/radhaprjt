using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.WorkCenter.Command.CreateWorkCenter
{
    public class CreateWorkCenterCommand :IRequest<ApiResponseDTO<int>> 
    {
        public string? WorkCenterCode { get; set; }
        public string? WorkCenterName { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
    }
}