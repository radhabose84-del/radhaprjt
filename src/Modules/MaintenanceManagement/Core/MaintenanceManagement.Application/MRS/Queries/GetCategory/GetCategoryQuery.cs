using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.MRS.Queries.GetCategory
{
    public class GetCategoryQuery : IRequest<List<MCategoryDto>>
    {
        public string? OldUnitcode { get; set; }   
    }
}