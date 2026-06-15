using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesContact.Commands.UpdateSalesContact
{
    public class UpdateSalesContactCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public string? ContactName { get; set; }
        public string? MobileNumber { get; set; }
        public int ContactTypeId { get; set; }
        public int? PartyId { get; set; }
        public string? Email { get; set; }
        public string? Remarks { get; set; }
        public int IsActive { get; set; }  // 1=Active, 0=Inactive
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
