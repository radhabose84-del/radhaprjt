using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.WorkCenter.Command.DeleteWorkCenter
{
    public class DeleteWorkCenterCommand : IRequest<ApiResponseDTO<int>> 
    {
        public int Id { get; set; }
    }
}