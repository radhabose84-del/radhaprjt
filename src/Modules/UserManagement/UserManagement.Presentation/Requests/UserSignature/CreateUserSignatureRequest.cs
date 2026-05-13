using Microsoft.AspNetCore.Http;

namespace UserManagement.Presentation.Requests.UserSignature
{
    public class CreateUserSignatureRequest
    {
        public int UserId { get; set; }
        public IFormFile? File { get; set; }
    }
}
