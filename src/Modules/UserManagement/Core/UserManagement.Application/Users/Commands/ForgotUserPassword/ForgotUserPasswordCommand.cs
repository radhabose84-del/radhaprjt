using Contracts.Common;
using MediatR;

namespace UserManagement.Application.Users.Commands.ForgotUserPassword
{
    public class ForgotUserPasswordCommand : IRequest<ApiResponseDTO<ForgotPasswordResponse>>
    {
         public string? UserName { get; set; }
    }
}