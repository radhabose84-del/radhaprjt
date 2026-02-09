using MediatR;

namespace BudgetManagement.Application.BudgetRequest.Commands.DeleteImage
{
    public class DeleteFileCommand : IRequest<bool>
    {        public string? imagePath { get; set; }       
    }
}