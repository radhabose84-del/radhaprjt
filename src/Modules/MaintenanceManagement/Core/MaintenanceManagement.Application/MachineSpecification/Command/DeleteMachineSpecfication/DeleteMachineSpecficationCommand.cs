using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.MachineSpecification.DeleteMachineSpecfication
{
    public class DeleteMachineSpecficationCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; } 
    }
}