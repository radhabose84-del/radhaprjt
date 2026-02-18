using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MaintenanceManagement.Application.MRS.Queries.GetDepartment;
using MediatR;

namespace MaintenanceManagement.Application.MRS.Queries
{
     public class GetDepartmentbyIdQuery : IRequest<List<MDepartmentDto>>
    {
        public string? OldUnitcode { get; set; }
    }
}