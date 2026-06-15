using MediatR;
using UserManagement.Application.AccessPolicy.Dto;

namespace UserManagement.Application.AccessPolicy.Queries.GetAccessPolicyById
{
    public class GetAccessPolicyByIdQuery : IRequest<AccessPolicyDto?>
    {
        public int Id { get; set; }
    }
}
