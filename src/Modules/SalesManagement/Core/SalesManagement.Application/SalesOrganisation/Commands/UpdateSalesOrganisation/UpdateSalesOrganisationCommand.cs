using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesOrganisation.Commands.UpdateSalesOrganisation
{
    public class UpdateSalesOrganisationCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public string? SalesOrganisationName { get; set; }
        public int CompanyId { get; set; }
        public string? Description { get; set; }
        public int IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
