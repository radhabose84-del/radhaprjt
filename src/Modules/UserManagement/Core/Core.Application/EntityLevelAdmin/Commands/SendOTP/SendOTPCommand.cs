using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.EntityLevelAdmin.Commands.SendOTP
{
    public class SendOTPCommand : IRequest<SendOTPDTO>
    {
        public string? Email { get; set; }
    }
}