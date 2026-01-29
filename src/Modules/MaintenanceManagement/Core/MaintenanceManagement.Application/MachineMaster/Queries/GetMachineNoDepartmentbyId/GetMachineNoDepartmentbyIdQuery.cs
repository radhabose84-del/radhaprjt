using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.MachineMaster.Queries.GetMachineNoDepartmentbyId
{
    public class GetMachineNoDepartmentbyIdQuery : IRequest<List<GetMachineNoDepartmentbyIdDto>>
    {
        public int DepartmentId { get; set; }
    }
}