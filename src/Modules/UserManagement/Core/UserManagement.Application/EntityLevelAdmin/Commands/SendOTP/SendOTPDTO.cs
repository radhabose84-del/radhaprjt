namespace UserManagement.Application.EntityLevelAdmin.Commands.SendOTP
{
    public class SendOTPDTO
    {
         public string? Email { get; set; }
         public string? VerificationCode { get; set; }
         public int  PasswordResetCodeExpiryMinutes  {get; set;}
    }
}