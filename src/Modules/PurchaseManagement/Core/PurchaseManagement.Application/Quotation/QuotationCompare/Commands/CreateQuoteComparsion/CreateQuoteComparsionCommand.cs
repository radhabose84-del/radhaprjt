using MediatR;
using Contracts.Common;

namespace PurchaseManagement.Application.Quotation.QuotationCompare.Commands.CreateQuoteComparsion
{
    public class CreateQuoteComparsionCommand : IRequest<int>, IRequirePermission
    {
        public CreateQuoteComparsionDto CreateQuoteComparsion { get; set; } = null!;
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
    
}
