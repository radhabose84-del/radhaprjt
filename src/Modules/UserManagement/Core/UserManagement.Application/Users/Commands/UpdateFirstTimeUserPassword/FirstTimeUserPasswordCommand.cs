using Contracts.Common;
using MediatR;

namespace UserManagement.Application.Users.Commands.UpdateFirstTimeUserPassword
{
    public class FirstTimeUserPasswordCommand : IRequest<ApiResponseDTO<string>>
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }
}