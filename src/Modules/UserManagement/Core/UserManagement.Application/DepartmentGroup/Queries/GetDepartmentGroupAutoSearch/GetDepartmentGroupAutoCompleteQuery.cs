using MediatR;

namespace UserManagement.Application.DepartmentGroup.Queries.GetDepartmentGroupAutoSearch
{
    public class GetDepartmentGroupAutoCompleteQuery   : IRequest<List<DepartmentGroupAutoCompleteDto>>
    {
          public string? SearchPattern { get; set; }
    }
}