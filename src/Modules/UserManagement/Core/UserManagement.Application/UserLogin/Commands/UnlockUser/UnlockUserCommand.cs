using UserManagement.Application.Common.HttpResponse;
using MediatR;

namespace UserManagement.Application.UserLogin.Commands.UnlockUser
{
    public class UnlockUserCommand : IRequest<bool>
    {
        public string? userName { get; set; }        
    }
}