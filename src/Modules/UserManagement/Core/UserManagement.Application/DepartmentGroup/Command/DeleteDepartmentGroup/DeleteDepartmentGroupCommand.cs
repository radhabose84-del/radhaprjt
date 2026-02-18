using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.DepartmentGroup.Command.DeleteDepartmentGroup
{
    public class DeleteDepartmentGroupCommand  :IRequest<bool> 
    {
         public int Id { get; set; }
    }
}