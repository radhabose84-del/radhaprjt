namespace SalesManagement.Application.LeadConversionFunnel.Dto
{
    public class LeadConversionFunnelDto
    {
        public List<OfficerFunnelDto> Officers { get; set; } = new();
    }

    public class OfficerFunnelDto
    {
        public int MarketingOfficerId { get; set; }
        public string? EmployeeNo { get; set; }
        public string? EmployeeName { get; set; }
        public List<CustomerFunnelDto> Customers { get; set; } = new();
    }

    public class CustomerFunnelDto
    {
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public List<FunnelLeadDto> Leads { get; set; } = new();
        public List<FunnelEnquiryDto> Enquiries { get; set; } = new();
        public List<FunnelQuotationDto> Quotations { get; set; } = new();
    }

    public class FunnelLeadDto
    {
        public int Id { get; set; }
        public DateTimeOffset InteractionDate { get; set; }
        public string? ProspectCompanyName { get; set; }
        public string? ContactName { get; set; }
        public string? MobileNumber { get; set; }
        public int? ItemId { get; set; }
        public string? ItemName { get; set; }
        public decimal? RequirementQty { get; set; }
        public string? LeadSourceName { get; set; }
        public string? Remarks { get; set; }
    }

    public class FunnelEnquiryDto
    {
        public int Id { get; set; }
        public DateTimeOffset EnquiryDate { get; set; }
        public string? ContactPerson { get; set; }
        public int? SalesLeadId { get; set; }
        public DateTimeOffset? ExpectedDeliveryDate { get; set; }
        public string? Remarks { get; set; }
    }

    public class FunnelQuotationDto
    {
        public int Id { get; set; }
        public DateOnly QuotationDate { get; set; }
        public DateOnly ValidityDate { get; set; }
        public decimal GrandTotal { get; set; }
        public int? SalesEnquiryId { get; set; }
        public string? StatusName { get; set; }
    }
}
