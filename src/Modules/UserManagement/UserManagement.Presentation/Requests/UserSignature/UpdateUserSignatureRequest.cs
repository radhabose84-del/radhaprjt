using Microsoft.AspNetCore.Http;

namespace UserManagement.Presentation.Requests.UserSignature
{
    public class UpdateUserSignatureRequest
    {
        public IFormFile? File { get; set; }
        public int IsActive { get; set; }
    }
}
