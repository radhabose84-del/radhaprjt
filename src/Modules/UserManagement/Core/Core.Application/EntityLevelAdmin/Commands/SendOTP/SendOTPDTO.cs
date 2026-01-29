using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.EntityLevelAdmin.Commands.SendOTP
{
    public class SendOTPDTO
    {
         public string? Email { get; set; }
         public string? VerificationCode { get; set; }
         public int  PasswordResetCodeExpiryMinutes  {get; set;}
    }
}