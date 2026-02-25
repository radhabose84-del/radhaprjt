using UserManagement.Application.Users.Queries.GetUsers;
using MediatR;

namespace UserManagement.Application.Users.Queries.GetUserById
{
    public class GetUserByIdQuery : IRequest<UserByIdDTO>
    {
        public int UserId { get; set; }
    }
}   