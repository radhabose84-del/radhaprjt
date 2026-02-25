using Contracts.Common;
using MediatR;

namespace UserManagement.Application.DepartmentGroup.Queries.GetAllDepartmentGroup
{
    public class GetAllDepartmentGroupQuery  :  IRequest<ApiResponseDTO<List<GetAllDepartmentGroupDto>>>
    { 
          public int PageNumber { get; set; } = 1;
          public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}