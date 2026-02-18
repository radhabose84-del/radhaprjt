using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MRS.Queries.GetSubDepartment
{
    public class GetSubDepartmentQuery : IRequest<List<MSubDepartment>>
    {
        public string? OldUnitcode { get; set; }   
    }
}