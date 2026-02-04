using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using MediatR;

namespace UserManagement.Application.DepartmentGroup.Command.CreateDepartmentGroup
{
    public class CreateDepartmentGroupCommand : IRequest<int>
    {   
        public string? DepartmentGroupCode { get; set; }
        public string? DepartmentGroupName { get; set; }       
        public byte IsActive { get; set; }
  
    }
}