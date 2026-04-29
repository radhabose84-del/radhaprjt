namespace SalesManagement.Application.SalesEnquiry.Dto
{
    public class SalesEnquiryHeaderDto
    {
        public int Id { get; set; }
        public string? EnquiryNo { get; set; }
        public int PartyId { get; set; }
        public string? PartyName { get; set; }
        public DateTimeOffset EnquiryDate { get; set; }
        public int EnquiryTypeId { get; set; }
        public string? EnquiryTypeCode { get; set; }
        public string? EnquiryTypeDescription { get; set; }
        public string? ContactPerson { get; set; }
        public DateTimeOffset? ExpectedDeliveryDate { get; set; }
        public int? PaymentTermId { get; set; }
        public string? PaymentTermDescription { get; set; }
        public int? SalesLeadId { get; set; }
        public string? SalesLeadProspectName { get; set; }
        public string? Remarks { get; set; }
        public bool IsEditable { get; set; }           // false when quotation OR sales order exists against this enquiry
        public string? QuotationNo { get; set; }        // populated via OUTER APPLY → SalesQuotationHeader
        public string? SalesOrderNo { get; set; }       // populated via OUTER APPLY → SalesOrderHeader (through quotation)
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public List<SalesEnquiryDetailDto>? SalesEnquiryDetails { get; set; }
    }
}
