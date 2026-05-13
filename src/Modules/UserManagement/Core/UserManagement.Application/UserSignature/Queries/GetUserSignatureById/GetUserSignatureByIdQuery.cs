using MediatR;

namespace UserManagement.Application.UserSignature.Queries.GetUserSignatureById
{
    public class GetUserSignatureByIdQuery : IRequest<UserSignatureByIdDto>
    {
        public int Id { get; set; }
    }
}
