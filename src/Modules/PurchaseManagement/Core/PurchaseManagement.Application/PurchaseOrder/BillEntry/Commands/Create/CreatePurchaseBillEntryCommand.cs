using PurchaseManagement.Application.PurchaseOrder.BillEntry.Dto;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.BillEntry.Commands.Create;

  public sealed record CreatePurchaseBillEntryCommand(
        PurchaseBillEntryHeaderDto Data
    ) : IRequest<int>;