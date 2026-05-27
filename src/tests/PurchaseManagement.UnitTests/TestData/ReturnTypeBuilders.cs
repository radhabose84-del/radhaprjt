using PurchaseManagement.Application.PurchaseReturn.ReturnType.Commands.CreateReturnType;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Commands.DeleteReturnType;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Commands.UpdateReturnType;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Dto;
using DomainBase = PurchaseManagement.Domain.Common.BaseEntity;
using DomainReturnType = PurchaseManagement.Domain.Entities.PurchaseReturn.ReturnType;

namespace PurchaseManagement.UnitTests.TestData;

public static class ReturnTypeBuilders
{
    public static CreateReturnTypeCommand ValidCreateCommand(
        string code = "Rejected",
        string description = "Rejected",
        int? inventoryImpactId = 1,
        int? financeImpactId = 1,
        bool isReplacementApplicable = false,
        bool isQcMandatory = true,
        string? approvalRoleCode = "QcHead") =>
        new(code, description, inventoryImpactId, financeImpactId, isReplacementApplicable, isQcMandatory, approvalRoleCode);

    public static UpdateReturnTypeCommand ValidUpdateCommand(
        int id = 1,
        string description = "Rejected Updated",
        int? inventoryImpactId = 1,
        int? financeImpactId = 1,
        bool isReplacementApplicable = false,
        bool isQcMandatory = true,
        string? approvalRoleCode = "QcHead",
        int isActive = 1) =>
        new(id, description, inventoryImpactId, financeImpactId, isReplacementApplicable, isQcMandatory, approvalRoleCode, isActive);

    public static DeleteReturnTypeCommand ValidDeleteCommand(int id = 1) =>
        new(id);

    public static ReturnTypeDto ValidDto(int id = 1) =>
        new()
        {
            Id = id,
            Code = "Rejected",
            Description = "Rejected",
            InventoryImpactId = 1,
            InventoryImpactName = "MoveToRtv",
            FinanceImpactId = 1,
            FinanceImpactName = "DebitNote",
            IsReplacementApplicable = false,
            IsQcMandatory = true,
            ApprovalRoleCode = "QcHead",
            IsActive = true,
            IsDeleted = false
        };

    public static ReturnTypeLookupDto ValidLookupDto(int id = 1) =>
        new()
        {
            Id = id,
            Code = "Rejected",
            Description = "Rejected",
            IsReplacementApplicable = false,
            IsQcMandatory = true
        };

    public static DomainReturnType ValidEntity(int id = 1) =>
        new()
        {
            Id = id,
            Code = "Rejected",
            Description = "Rejected",
            InventoryImpactId = 1,
            FinanceImpactId = 1,
            IsReplacementApplicable = false,
            IsQcMandatory = true,
            ApprovalRoleCode = "QcHead",
            IsActive = DomainBase.Status.Active,
            IsDeleted = DomainBase.IsDelete.NotDeleted
        };
}
