using MediatR;

namespace UserManagement.Application.UserSignature.Command.CreateUserSignature
{
    public class CreateUserSignatureCommand : IRequest<int>
    {
        public int UserId { get; set; }
        public byte[]? SignatureImage { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public int FileSizeBytes { get; set; }
    }
}
