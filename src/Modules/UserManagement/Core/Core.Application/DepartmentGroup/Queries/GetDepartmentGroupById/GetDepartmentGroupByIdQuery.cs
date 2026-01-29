using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.DepartmentGroup.Queries.GetDepartmentGroupById
{
    public class GetDepartmentGroupByIdQuery : IRequest<DepartmentGroupByIdDto>
    {
        public int Id { get; set; }
    }
    
   
}