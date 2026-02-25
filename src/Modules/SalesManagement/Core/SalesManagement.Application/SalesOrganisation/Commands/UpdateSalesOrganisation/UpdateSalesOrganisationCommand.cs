using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesOrganisation.Commands.UpdateSalesOrganisation
{
    public class UpdateSalesOrganisationCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string SalesOrganisationName { get; set; } = null!;
        public int CompanyId { get; set; }
        public string Description { get; set; } = null!;
        public int IsActive { get; set; }
    }
}
