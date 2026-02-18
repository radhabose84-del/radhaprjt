using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.UserLogin.Commands.DeactivateUserSession
{
    public class DeactivateUserSessionCommand : IRequest<bool>
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}