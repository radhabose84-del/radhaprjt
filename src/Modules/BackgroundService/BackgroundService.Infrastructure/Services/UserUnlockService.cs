using BackgroundService.Application.Interfaces;
using MediatR;
using UserManagement.Application.UserLogin.Commands.UnlockUser;

namespace BackgroundService.Infrastructure.Services
{
    public class UserUnlockService : IUserUnlockService
    {
        private readonly IMediator _mediator;

        public UserUnlockService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task UnlockUser(string userName) =>
            _mediator.Send(new UnlockUserCommand { userName = userName });
    }
}
