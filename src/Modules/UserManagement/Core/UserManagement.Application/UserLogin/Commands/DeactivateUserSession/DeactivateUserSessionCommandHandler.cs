#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using UserManagement.Application.Common.Interfaces.IUserSession;
using MediatR;
using Serilog;

namespace UserManagement.Application.UserLogin.Commands.DeactivateUserSession
{
    public class DeactivateUserSessionCommandHandler : IRequestHandler<DeactivateUserSessionCommand, bool>
    {
        private readonly IUserSessionRepository _userSessionRepository;
        public DeactivateUserSessionCommandHandler(IUserSessionRepository userSessionRepository)
        {
            _userSessionRepository = userSessionRepository;
        }
        public async Task<bool> Handle(DeactivateUserSessionCommand request, CancellationToken cancellationToken)
        {
            var session = await _userSessionRepository.DeactivateUserSessionsByUsername(request.Username);
            // if (session)
            // {
            //     return session;
            // }
            // throw new Exception("Session Deactivation Failed.");
            // ✅ NEW CODE:
            if (!session)
            {
                Log.Information("No active sessions found for user: {Username}", request.Username);
            }

            return session; // Don't throw - just return the result

        }
    }
}