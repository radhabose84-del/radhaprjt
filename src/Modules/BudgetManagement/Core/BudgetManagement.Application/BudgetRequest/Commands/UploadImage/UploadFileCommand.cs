using BudgetManagement.Application.BudgetRequest.Commands.UploadImage;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace BudgetManagement.Application.BudgetRequest.Commands
{
    public class UploadFileCommand : IRequest<ImageDto>
    {
        public IFormFile? File { get; set; }
    }
}
