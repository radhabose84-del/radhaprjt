using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Users.Commands.ForgotUserPassword
{
    public class ForgotPasswordResponse
    {
    public string? Message { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? VerificationCode { get; set; }
    public int  PasswordResetCodeExpiryMinutes  {get; set;}

    }
}