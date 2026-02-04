using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using MediatR;

namespace UserManagement.Application.DepartmentGroup.Command.DeleteDepartmentGroup
{
    public class DeleteDepartmentGroupCommand  :IRequest<bool> 
    {
         public int Id { get; set; }
    }
}