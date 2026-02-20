#nullable disable
using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesOrganisation.Commands.CreateSalesOrganisation
{
    public class CreateSalesOrganisationCommand : IRequest<ApiResponseDTO<int>>
    {
        public string SalesOrganisationCode { get; set; }
        public string SalesOrganisationName { get; set; }
        public int CompanyId { get; set; }
    }
}
