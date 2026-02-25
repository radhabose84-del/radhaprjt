using Contracts.Common;
using MediatR;

namespace UserManagement.Application.UserLogin.Commands.UserLogin
{
    public class UserLoginCommand : IRequest<ApiResponseDTO<LoginResponse>>
    {
        // public LoginRequest Request { get; set; }
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;

    }

}