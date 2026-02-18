using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MachineSpecification.DeleteMachineSpecfication
{
    public class DeleteMachineSpecficationCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; } 
    }
}