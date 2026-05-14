using MediatR;
using UserManagement.Application.UserSignature.Queries.GetUserSignatureById;

namespace UserManagement.Application.UserSignature.Queries.GetUserSignatureByUserId
{
    public class GetUserSignatureByUserIdQuery : IRequest<UserSignatureByIdDto>
    {
        public int UserId { get; set; }
    }
}
