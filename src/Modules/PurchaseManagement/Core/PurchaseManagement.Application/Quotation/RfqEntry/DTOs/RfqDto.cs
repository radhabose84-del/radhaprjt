
namespace PurchaseManagement.Application.Quotation.RfqEntry.DTOs;
public record RfqDto(
    int Id,
    int? UnitId,
    string RfqCode,
    int RfqStatusId,
    string RfqStatusDesc,
    int InitiationTypeId,
    string InitiationTypeDesc,
    int? IndentId,
    DateOnly LastSubmitDate,
    RfqItemDto[] Items,
    RfqSupplierDto[] Suppliers,
    RfqAttachmentDto[] Attachments);