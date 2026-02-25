using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesOrganisation.Commands.CreateSalesOrganisation
{
    public class CreateSalesOrganisationCommand : IRequest<ApiResponseDTO<int>>
    {
        public string SalesOrganisationCode { get; set; } = null!;
        public string SalesOrganisationName { get; set; } = null!;
        public int CompanyId { get; set; }
        public string Description { get; set; } = null!;
    }
}
