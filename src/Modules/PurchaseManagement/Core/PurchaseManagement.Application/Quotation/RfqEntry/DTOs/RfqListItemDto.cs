namespace PurchaseManagement.Application.Quotation.RfqEntry.DTOs;

public record RfqListItemDto(
    int Id,
    int? UnitId,
    string RfqCode,
    int RfqStatusId,
    string RfqStatusDesc,
    int? InitiationTypeId,
    string InitiationTypeDesc,
    int? IndentId,
    DateOnly? LastSubmissionDate,
    int Edit,
    string? EditReason,
    string RfqFlagStatus
);
