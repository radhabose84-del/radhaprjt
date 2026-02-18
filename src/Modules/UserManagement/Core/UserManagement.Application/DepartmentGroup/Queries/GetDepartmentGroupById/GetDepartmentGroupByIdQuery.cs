using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.DepartmentGroup.Queries.GetDepartmentGroupById
{
    public class GetDepartmentGroupByIdQuery : IRequest<DepartmentGroupByIdDto>
    {
        public int Id { get; set; }
    }
    
   
}