using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.EntityLevelAdmin.Commands.ResetPassword
{
    public class ResetPasswordCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public string? VerificationCode { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}