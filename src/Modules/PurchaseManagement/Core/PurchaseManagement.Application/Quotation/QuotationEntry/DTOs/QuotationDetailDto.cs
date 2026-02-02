    namespace PurchaseManagement.Application.Quotations.QuotationEntry.DTOs;

    public record QuotationDetailDto(
        int ItemId,
        int HsnId,
        int UomId,
        int CurrencyId,
        decimal Quantity,
        decimal Rate,
        int? DiscountTypeId,
        decimal Discount,
        decimal PandFCharge,
        decimal GstPercent,
        decimal Warranty, decimal ValidityDays,decimal DeliveryDays, decimal LineSubtotal, decimal GstAmount, decimal Total,
        int  IsActive
    );
    public record GetQuotationHeaderDto(
        int Id,
        string QuotationNumber,
        int SupplierId,
        string SupplierName,
        int RfqId,
        string RfqNumber,
        DateOnly ValidTill,
        int FreightModeId,
        string FreightModeName,
        decimal Freight,
        int PaymentTermsId,
        string PaymentTermsName,
        int IncotermsId,
        string IncotermsName,  
        decimal InsuranceCharge,      
        decimal TaxableSubtotal,
        decimal GstTotal,
        decimal ItemsTotal,
        decimal GrandTotal,
        int  IsActive,
        string? QuotationImage,
        string? ImageUrl,
        IReadOnlyList<GetQuotationDetailDto> Lines
    );

    public record GetQuotationDetailDto(
        int ItemId,string ItemCode, string ItemName,   
        int HsnId,
        int UomId,int CurrencyId,string CurrencyName,  string UomName,
        decimal Quantity,
        decimal Rate,
        int DiscountTypeId,
        decimal Discount,
        decimal PandFCharge,
        decimal GstPercent,
        decimal Warranty, decimal ValidityDays,decimal DeliveryDays, decimal LineSubtotal, decimal GstAmount, decimal Total,
        int  IsActive
    );
