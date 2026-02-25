using MediatR;

namespace UserManagement.Application.Entity.Queries.GetEntityBasedCompany
{
    public class GetEntityBasedCompanyQuery : IRequest<List<GetEntityBasedCompanyDto>>
    {
         public int EntityId { get; set; } 
    }
}