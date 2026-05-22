using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.EInvoiceHeader.Commands.DeleteEInvoiceHeader
{
    public sealed record DeleteEInvoiceHeaderCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
