using MediatR;

namespace UserManagement.Application.UserSignature.Command.DeleteUserSignature
{
    public class DeleteUserSignatureCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}
