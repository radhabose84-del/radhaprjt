using MediatR;

namespace UserManagement.Application.Companies.Commands.DeleteFileCompany
{
    public class DeleteFileCompanyCommand : IRequest<bool>
    {
        public string? Logo { get; set; }
    }
}