using MediatR;
using Contracts.Common;

namespace PartyManagement.Application.PartyGroup.Command.UpdatePartyGroup
{
    public class UpdatePartyGroupCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        public string? PartyGroupName { get; set; }
        public int? ParentPartyGroupId { get; set; }
        public string? Description { get; set; }
        public string? Glcode { get; set; }
        public int GlCategoryId { get; set; }
        public byte IsGroup { get; set; }
        public byte IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
