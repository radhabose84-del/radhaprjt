using MediatR;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Dto;

namespace PurchaseManagement.Application.PurchaseReturn.ReturnType.Commands.UpdateReturnType;

public sealed record UpdateReturnTypeCommand(
    int Id,
    string Description,
    int? InventoryImpactId,
    int? FinanceImpactId,
    bool IsReplacementApplicable,
    bool IsQcMandatory,
    string? ApprovalRoleCode,
    int IsActive
) : IRequest<ReturnTypeDto>;
