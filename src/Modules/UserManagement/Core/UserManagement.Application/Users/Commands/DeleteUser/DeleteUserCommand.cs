using Contracts.Common;
using MediatR;

namespace UserManagement.Application.Users.Commands.DeleteUser
{
    public class DeleteUserCommand : IRequest<ApiResponseDTO<bool>>
    {
        public int UserId { get; set; }
    }
}
