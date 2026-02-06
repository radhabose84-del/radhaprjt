using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using MediatR;

namespace UserManagement.Application.EntityLevelAdmin.Commands.SendOTP
{
    public class SendOTPCommand : IRequest<SendOTPDTO>
    {
        public string? Email { get; set; }
    }
}