using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.CancelPurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.CreatePurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.DeletePurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.ProcessApprovalDecision;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.SubmitPurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.UpdatePurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;
using PurchaseManagement.Domain.Entities.PurchaseReturn;
using DomainBase = PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.TestData;

public static class PurchaseReturnBuilders
{
    public static CreatePurchaseReturnDetailDto ValidDetailInput(
        int grnDetailId = 1,
        int itemId = 1,
        int uomId = 1,
        decimal returnQty = 5m,
        decimal acceptedQty = 10m,
        int? returnReasonId = null) =>
        new()
        {
            GrnDetailId = grnDetailId,
            ItemId = itemId,
            UomId = uomId,
            ReceivedQty = acceptedQty,
            AcceptedQty = acceptedQty,
            ReturnQty = returnQty,
            RatePerUnit = 100m,
            LineValue = returnQty * 100m,
            ReturnReasonId = returnReasonId,
            LineRemarks = null
        };

    public static CreatePurchaseReturnCommand ValidCreateCommand(
        int vendorId = 42,
        int poId = 100,
        int grnHeaderId = 200,
        int returnTypeId = 1,
        int returnReasonId = 1,
        int returnActionId = 1) =>
        new()
        {
            RtvDate = new DateOnly(2026, 5, 27),
            UnitId = 37,
            VendorId = vendorId,
            PoId = poId,
            GrnHeaderId = grnHeaderId,
            ReturnTypeId = returnTypeId,
            ReturnReasonId = returnReasonId,
            ReturnActionId = returnActionId,
            IsReplacementRequired = false,
            IsDebitNoteRequired = true,
            IsQcVerified = true,
            Remarks = "Test RTV",
            Details = new List<CreatePurchaseReturnDetailDto> { ValidDetailInput() }
        };

    public static UpdatePurchaseReturnCommand ValidUpdateCommand(int id = 1) =>
        new()
        {
            Id = id,
            RtvDate = new DateOnly(2026, 5, 27),
            UnitId = 37,
            VendorId = 42,
            PoId = 100,
            GrnHeaderId = 200,
            ReturnTypeId = 1,
            ReturnReasonId = 1,
            ReturnActionId = 1,
            IsReplacementRequired = false,
            IsDebitNoteRequired = true,
            IsQcVerified = true,
            Remarks = "Updated",
            IsActive = 1,
            Details = new List<CreatePurchaseReturnDetailDto> { ValidDetailInput() }
        };

    public static SubmitPurchaseReturnCommand ValidSubmitCommand(int id = 1) => new(id);
    public static CancelPurchaseReturnCommand ValidCancelCommand(int id = 1) => new(id);
    public static DeletePurchaseReturnCommand ValidDeleteCommand(int id = 1) => new(id);

    public static ProcessPurchaseReturnApprovalDecisionCommand ValidProcessApprovalCommand(
        int id = 1,
        bool isApproved = true) =>
        new(id, isApproved, ApprovalRequestId: 99, ApproverRemarks: null);

    public static PurchaseReturnHeaderDto ValidHeaderDto(int id = 1, string statusCode = "Draft") =>
        new()
        {
            Id = id,
            RtvNumber = "RTV/2026/0001",
            RtvDate = new DateOnly(2026, 5, 27),
            UnitId = 37,
            VendorId = 42,
            VendorName = "ACME Supplies",
            PoId = 100,
            PoNumber = "PO/2026/0001",
            GrnHeaderId = 200,
            GrnNo = "GRN/2026/0001",
            ReturnTypeId = 1,
            ReturnTypeCode = "Rejected",
            ReturnReasonId = 1,
            ReturnReasonCode = "MoistureFailure",
            ReturnActionId = 1,
            ReturnActionCode = "ReturnToVendor",
            StatusId = 1,
            StatusCode = statusCode,
            IsActive = true,
            IsDeleted = false,
            Details = new List<PurchaseReturnDetailDto>()
        };

    public static PurchaseReturnListItemDto ValidListItemDto(int id = 1) =>
        new()
        {
            Id = id,
            RtvNumber = "RTV/2026/0001",
            RtvDate = new DateOnly(2026, 5, 27),
            VendorId = 42,
            VendorName = "ACME Supplies",
            PoId = 100,
            PoNumber = "PO/2026/0001",
            GrnHeaderId = 200,
            GrnNo = "GRN/2026/0001",
            ReturnTypeId = 1,
            ReturnTypeCode = "Rejected",
            ReturnReasonId = 1,
            ReturnReasonCode = "MoistureFailure",
            StatusId = 1,
            StatusCode = "Draft",
            IsActive = true
        };

    public static ReturnableQtyDto ValidReturnableQtyDto(int grnDetailId = 1) =>
        new()
        {
            GrnDetailId = grnDetailId,
            ItemId = 1,
            ItemCode = "ITEM001",
            ItemName = "Test Item",
            UomId = 1,
            UomName = "KG",
            ReceivedQty = 100m,
            AcceptedQty = 80m,
            PriorReturnedQty = 5m,
            ReturnableQty = 75m,
            RatePerUnit = 100m
        };

    public static PurchaseReturnHeader ValidHeaderEntity(int id = 1) =>
        new()
        {
            Id = id,
            RtvNumber = "RTV/2026/0001",
            RtvDate = new DateOnly(2026, 5, 27),
            UnitId = 37,
            VendorId = 42,
            PoId = 100,
            GrnHeaderId = 200,
            ReturnTypeId = 1,
            ReturnReasonId = 1,
            ReturnActionId = 1,
            StatusId = 1,
            IsActive = DomainBase.Status.Active,
            IsDeleted = DomainBase.IsDelete.NotDeleted,
            Details = new List<PurchaseReturnDetail>()
        };
}
