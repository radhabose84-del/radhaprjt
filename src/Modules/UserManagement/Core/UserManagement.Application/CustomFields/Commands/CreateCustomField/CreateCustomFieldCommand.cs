using MediatR;
using Contracts.Common;

namespace UserManagement.Application.CustomFields.Commands.CreateCustomField
{
    public class CreateCustomFieldCommand : IRequest<int>, IRequirePermission
    {
        public string? LabelName { get; set; }
        public int Length { get; set; }
        public int DataTypeId { get; set; }
        public int LabelTypeId { get; set; }
        public byte IsRequired { get; set; }
        public List<CustomFieldMenuDto>? Menu { get; set; }
        public List<CustomFieldUnitDto>? Unit { get; set; }
        public List<CustomFieldOptionalValueDto>? OptionalValues { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
