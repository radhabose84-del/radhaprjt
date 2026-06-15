using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.CountMaster.Commands.UpdateCountMaster
{
    public class UpdateCountMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public decimal CountValue { get; set; }
        public string? ShortName { get; set; }
        public int? CountCategoryId { get; set; }
        public int CountTypeId { get; set; }
        public string? CountDescription { get; set; }
        public int UOMId { get; set; }
        public int IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
