using MediatR;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Dto;

namespace PurchaseManagement.Application.PurchaseReturn.ReturnType.Commands.CreateReturnType;

public sealed record CreateReturnTypeCommand(
    string Code,
    string Description,
    int? InventoryImpactId,
    int? FinanceImpactId,
    bool IsReplacementApplicable,
    bool IsQcMandatory,
    string? ApprovalRoleCode
) : IRequest<ReturnTypeDto>;
