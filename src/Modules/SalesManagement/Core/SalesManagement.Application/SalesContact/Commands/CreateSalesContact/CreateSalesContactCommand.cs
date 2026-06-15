using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesContact.Commands.CreateSalesContact
{
    public class CreateSalesContactCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public string? ContactName { get; set; }
        public string? MobileNumber { get; set; }
        public int ContactTypeId { get; set; }
        public int? PartyId { get; set; }
        public string? Email { get; set; }
        public string? Remarks { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
