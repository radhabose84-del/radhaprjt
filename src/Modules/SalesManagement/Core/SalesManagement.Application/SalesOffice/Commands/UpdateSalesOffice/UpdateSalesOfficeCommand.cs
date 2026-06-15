using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesOffice.Commands.UpdateSalesOffice
{
    public class UpdateSalesOfficeCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public string? SalesOfficeName { get; set; }
        public int SalesOrganisationId { get; set; }
        public int CityId { get; set; }
        public string? Pincode { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? ResponsibleManager { get; set; }
        public string? RegionTerritory { get; set; }
        public string? Address { get; set; }
        public int IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
