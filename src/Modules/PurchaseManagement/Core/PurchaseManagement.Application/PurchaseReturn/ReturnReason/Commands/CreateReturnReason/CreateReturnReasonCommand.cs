using MediatR;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Dto;

namespace PurchaseManagement.Application.PurchaseReturn.ReturnReason.Commands.CreateReturnReason;

public sealed record CreateReturnReasonCommand(
    string Code,
    string Description,
    int ReturnTypeId,
    bool? IsReplacementOverride,
    bool? IsDebitNoteOverride,
    bool? IsQcMandatoryOverride
) : IRequest<ReturnReasonDto>;
