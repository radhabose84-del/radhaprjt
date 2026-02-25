using MediatR;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Application.Menu.Commands.UploadMenu
{
    public class UploadMenuCommand : IRequest<string>
    {
        public IFormFile File { get; set; } = default!;
    }
}