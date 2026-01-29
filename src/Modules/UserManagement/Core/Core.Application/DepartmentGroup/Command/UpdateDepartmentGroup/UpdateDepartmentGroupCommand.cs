using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using Core.Application.DepartmentGroup.Queries.GetDepartmentGroupById;
using MediatR;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.DepartmentGroup.Command.UpdateDepartmentGroup
{
    public class UpdateDepartmentGroupCommand  : IRequest<int>
    {
         public int Id { get; set; }
        public string? DepartmentGroupCode { get; set; }
        public string? DepartmentGroupName { get; set; }        
        public Status IsActive { get; set; }
    }
}