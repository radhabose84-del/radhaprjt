using Contracts.Common;
using MediatR;

namespace UserManagement.Application.Departments.Queries.GetDepartments
{

    public class GetDepartmentQuery : IRequest<ApiResponseDTO<List<GetDepartmentDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}