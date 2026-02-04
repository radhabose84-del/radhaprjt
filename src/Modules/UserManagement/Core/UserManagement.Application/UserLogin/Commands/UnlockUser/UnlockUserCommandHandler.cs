

using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Common.Interfaces.IUser;

using MediatR;

namespace UserManagement.Application.UserLogin.Commands.UnlockUser
{
    public class UnlockUserCommandHandler  : IRequestHandler<UnlockUserCommand, bool>
    {
        private readonly IUserCommandRepository _userSessionRepository;
        public UnlockUserCommandHandler(IUserCommandRepository userSessionRepository)
        {
            _userSessionRepository = userSessionRepository;
        }
        public async Task<bool> Handle(UnlockUserCommand request, CancellationToken cancellationToken)
        {
            var session = await _userSessionRepository.UnlockUser(request.userName);
            if (session)
            {
                return session ;
            }
            throw new Exception("User Unlocked  Failed.");
                       
        }
    }
}