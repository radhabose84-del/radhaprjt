    namespace PurchaseManagement.Application.Quotations.QuotationEntry.DTOs;

    public record QuotationAutoCompleteDto(int Id, string QuotationNumber,string SupplierName,int SupplierId, string RfqNumber,int RfqId);
