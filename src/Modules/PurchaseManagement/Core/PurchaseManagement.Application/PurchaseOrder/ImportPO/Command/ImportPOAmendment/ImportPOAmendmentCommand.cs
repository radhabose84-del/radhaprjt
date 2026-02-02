using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.ImportPOAmendment
{
    public sealed class ImportPOAmendmentCommand : IRequest<int>
    {
        public ImportPOUpdateDto Data { get; set; } = default!;
    }
}
