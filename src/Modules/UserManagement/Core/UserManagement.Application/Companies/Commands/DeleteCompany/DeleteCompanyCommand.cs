using MediatR;

namespace UserManagement.Application.Companies.Commands.DeleteCompany
{
    public class DeleteCompanyCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}