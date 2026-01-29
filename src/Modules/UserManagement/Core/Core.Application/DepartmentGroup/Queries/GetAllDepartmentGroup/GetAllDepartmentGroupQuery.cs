using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.DepartmentGroup.Queries.GetAllDepartmentGroup
{
    public class GetAllDepartmentGroupQuery  :  IRequest<ApiResponseDTO<List<GetAllDepartmentGroupDto>>>
    { 
          public int PageNumber { get; set; } = 1;
          public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}