using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.DepartmentGroup.Queries.GetDepartmentGroupById;
using MediatR;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.DepartmentGroup.Command.UpdateDepartmentGroup
{
    public class UpdateDepartmentGroupCommand  : IRequest<int>
    {
         public int Id { get; set; }
        public string? DepartmentGroupCode { get; set; }
        public string? DepartmentGroupName { get; set; }        
        public Status IsActive { get; set; }
    }
}