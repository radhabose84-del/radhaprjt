using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.WorkCenter.Command.UpdateWorkCenter
{
    public class UpdateWorkCenterCommand :IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string? WorkCenterName { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public byte IsActive { get; set; }
    }
}