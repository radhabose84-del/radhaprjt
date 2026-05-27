using MediatR;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Dto;

namespace PurchaseManagement.Application.PurchaseReturn.ReturnReason.Commands.UpdateReturnReason;

public sealed record UpdateReturnReasonCommand(
    int Id,
    string Description,
    int ReturnTypeId,
    bool? IsReplacementOverride,
    bool? IsDebitNoteOverride,
    bool? IsQcMandatoryOverride,
    int IsActive
) : IRequest<ReturnReasonDto>;
