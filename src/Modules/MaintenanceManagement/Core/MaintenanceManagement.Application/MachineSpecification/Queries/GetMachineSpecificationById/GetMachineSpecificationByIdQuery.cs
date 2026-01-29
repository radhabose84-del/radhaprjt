using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.MachineSpecification.Command;
using MediatR;

namespace MaintenanceManagement.Application.MachineSpecification.Queries.GetMachineSpecificationById
{
    public class GetMachineSpecificationByIdQuery : IRequest<ApiResponseDTO<List<MachineSpecificationDto>>>
    {
        public int Id { get; set; }
    }
}