using MediatR;
using Contracts.Common;

namespace UserManagement.Application.CustomFields.Commands.UpdateCustomField
{
    public class UpdateCustomFieldCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        public string? LabelName { get; set; }
        public int Length { get; set; }
        public int DataTypeId { get; set; }
        public int LabelTypeId { get; set; }
        public byte IsRequired { get; set; }
        public byte IsActive { get; set; }
        public List<CustomFieldMenuUpdateDto>? Menu { get; set; }
        public List<CustomFieldUnitUpdateDto>? Unit { get; set; }
        public List<CustomFieldOptionalValueUpdateDto>? OptionalValues { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
