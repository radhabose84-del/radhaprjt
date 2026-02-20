#nullable disable
using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesOrganisation.Commands.UpdateSalesOrganisation
{
    public class UpdateSalesOrganisationCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string SalesOrganisationName { get; set; }
        public int CompanyId { get; set; }
        public int IsActive { get; set; }
    }
}
