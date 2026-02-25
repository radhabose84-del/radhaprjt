using MediatR;

namespace UserManagement.Application.EntityLevelAdmin.Commands.SendOTP
{
    public class SendOTPCommand : IRequest<SendOTPDTO>
    {
        public string? Email { get; set; }
    }
}