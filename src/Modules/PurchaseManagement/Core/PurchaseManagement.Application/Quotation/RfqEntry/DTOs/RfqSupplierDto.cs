namespace PurchaseManagement.Application.Quotation.RfqEntry.DTOs;
public record RfqSupplierDto(int? SupplierId, string Name, string Email, string? Mobile, string? Gst);