using MediatR;
using Contracts.Common;

namespace SalesManagement.Application.SalesEnquiry.Commands.CreateSalesEnquiry
{
    public class CreateSalesEnquiryCommand : IRequest<int>, IRequirePermission
    {
        public CreateSalesEnquiryDto SalesEnquiryDetails { get; set; } = null!;
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
