namespace PurchaseManagement.Application.Quotations.QuotationEntry.DTOs;

public record QuotationListItemDto(
    int Id,
    string QuotationNumber,    
    string SupplierName,    
    string RfqNumber,
    DateOnly ValidTill,         
    string FreightModeName,
    decimal Freight,    
    string PaymentTermsName,    
    string IncotermsName,
    decimal InsuranceCharge,
    decimal TaxableSubtotal,
    decimal GstTotal,
    decimal ItemsTotal,
    decimal GrandTotal,
    int  IsActive,
    string? QuotationImage   ,int Edit ,    string? EditReason   
);
