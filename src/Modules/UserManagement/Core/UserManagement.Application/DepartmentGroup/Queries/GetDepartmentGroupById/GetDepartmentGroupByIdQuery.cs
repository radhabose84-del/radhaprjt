using MediatR;

namespace UserManagement.Application.DepartmentGroup.Queries.GetDepartmentGroupById
{
    public class GetDepartmentGroupByIdQuery : IRequest<DepartmentGroupByIdDto>
    {
        public int Id { get; set; }
    }
    
   
}