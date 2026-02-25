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