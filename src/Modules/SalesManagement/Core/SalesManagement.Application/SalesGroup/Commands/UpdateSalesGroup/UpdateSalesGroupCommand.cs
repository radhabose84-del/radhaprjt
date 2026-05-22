using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesGroup.Commands.UpdateSalesGroup
{
    public class UpdateSalesGroupCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public string? SalesGroupName { get; set; }
        public int SalesOfficeId { get; set; }
        public string? ResponsibleManager { get; set; }
        public int? ProductCategoryId { get; set; }
        public string? RegionTerritory { get; set; }
        public int IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
