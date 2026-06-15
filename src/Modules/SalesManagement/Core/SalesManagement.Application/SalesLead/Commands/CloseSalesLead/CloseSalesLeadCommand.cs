using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesLead.Commands.CloseSalesLead
{
    public class CloseSalesLeadCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public int ClosureTypeId { get; set; }
        public int? ClosureReasonId { get; set; }       // required for all closure types except Won
        public int? ConvertWonLeadToId { get; set; }    // required only for Won
        public string? ClosureRemarks { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
