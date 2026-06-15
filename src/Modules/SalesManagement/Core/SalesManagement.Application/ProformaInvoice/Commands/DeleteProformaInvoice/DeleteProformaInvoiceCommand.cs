using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.ProformaInvoice.Commands.DeleteProformaInvoice;

public sealed record DeleteProformaInvoiceCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
