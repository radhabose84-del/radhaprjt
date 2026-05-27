using MediatR;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;

namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.CreatePurchaseReturn;

public class CreatePurchaseReturnCommand : IRequest<PurchaseReturnHeaderDto>
{
    public DateOnly RtvDate { get; set; }
    public int UnitId { get; set; }
    public int VendorId { get; set; }
    public int PoId { get; set; }
    public int GrnHeaderId { get; set; }
    public int ReturnTypeId { get; set; }
    public int ReturnReasonId { get; set; }
    public int ReturnActionId { get; set; }
    public bool IsReplacementRequired { get; set; }
    public bool IsDebitNoteRequired { get; set; }
    public bool IsQcVerified { get; set; }
    public string? Remarks { get; set; }
    public List<CreatePurchaseReturnDetailDto> Details { get; set; } = new();
}
