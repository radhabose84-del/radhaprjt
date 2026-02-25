using Contracts.Common;
using MediatR;

namespace UserManagement.Application.Users.Commands.ChangeUserPassword
{
    public class ChangeUserPasswordCommand : IRequest<ApiResponseDTO<string>>
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}