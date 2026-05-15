using MediatR;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Application.UserSignature.Command.CreateUserSignature
{
    public class CreateUserSignatureCommand : IRequest<int>
    {
        public int UserId { get; set; }
        public IFormFile? File { get; set; }
    }
}
