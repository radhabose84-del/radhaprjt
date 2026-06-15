using MediatR;
using Contracts.Common;

namespace PurchaseManagement.Application.PurchaseIndents.Command.CreatePurchaseIndent
{
    public class CreatePurchaseIndentCommand : IRequest<int>, IRequirePermission
    {
        
        public DateOnly IndentDate { get; set; }
        public int IndentTypeId { get; set; }
        public int UnitId { get; set; }
        public string? Purpose { get; set; }
        public int DepartmentId { get; set; }
        public byte IsDraft { get; set; }
        public ICollection<IndentDetailDto> IndentDetails { get; set; } = default!;
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
