using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Commands.CreateReturnReason;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Commands.DeleteReturnReason;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Commands.UpdateReturnReason;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Dto;
using DomainBase = PurchaseManagement.Domain.Common.BaseEntity;
using DomainReturnReason = PurchaseManagement.Domain.Entities.PurchaseReturn.ReturnReason;

namespace PurchaseManagement.UnitTests.TestData;

public static class ReturnReasonBuilders
{
    public static CreateReturnReasonCommand ValidCreateCommand(
        string code = "MoistureFailure",
        string description = "Moisture Failure",
        int returnTypeId = 1,
        bool? isReplacementOverride = null,
        bool? isDebitNoteOverride = null,
        bool? isQcMandatoryOverride = null) =>
        new(code, description, returnTypeId, isReplacementOverride, isDebitNoteOverride, isQcMandatoryOverride);

    public static UpdateReturnReasonCommand ValidUpdateCommand(
        int id = 1,
        string description = "Moisture Failure Updated",
        int returnTypeId = 1,
        bool? isReplacementOverride = null,
        bool? isDebitNoteOverride = null,
        bool? isQcMandatoryOverride = null,
        int isActive = 1) =>
        new(id, description, returnTypeId, isReplacementOverride, isDebitNoteOverride, isQcMandatoryOverride, isActive);

    public static DeleteReturnReasonCommand ValidDeleteCommand(int id = 1) =>
        new(id);

    public static ReturnReasonDto ValidDto(int id = 1) =>
        new()
        {
            Id = id,
            Code = "MoistureFailure",
            Description = "Moisture Failure",
            ReturnTypeId = 1,
            ReturnTypeName = "Rejected",
            IsActive = true,
            IsDeleted = false
        };

    public static ReturnReasonLookupDto ValidLookupDto(int id = 1) =>
        new()
        {
            Id = id,
            Code = "MoistureFailure",
            Description = "Moisture Failure",
            ReturnTypeId = 1
        };

    public static DomainReturnReason ValidEntity(int id = 1) =>
        new()
        {
            Id = id,
            Code = "MoistureFailure",
            Description = "Moisture Failure",
            ReturnTypeId = 1,
            IsActive = DomainBase.Status.Active,
            IsDeleted = DomainBase.IsDelete.NotDeleted
        };
}
