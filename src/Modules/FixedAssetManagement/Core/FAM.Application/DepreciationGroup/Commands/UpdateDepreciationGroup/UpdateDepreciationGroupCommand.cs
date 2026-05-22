using MediatR;
using static FAM.Domain.Common.BaseEntity;
using Contracts.Common;

namespace FAM.Application.DepreciationGroup.Commands.UpdateDepreciationGroup
{
    public class UpdateDepreciationGroupCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }       
        public string? Code { get; set; }
        public int BookType { get; set; }
        public string? DepreciationGroupName { get; set; }        
        public int AssetGroupId { get; set; } 
        public int? UsefulLife { get; set; }
        public int DepreciationMethod { get; set; }
        public int? ResidualValue { get; set; }
        public int SortOrder { get; set; }
        public Status IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
