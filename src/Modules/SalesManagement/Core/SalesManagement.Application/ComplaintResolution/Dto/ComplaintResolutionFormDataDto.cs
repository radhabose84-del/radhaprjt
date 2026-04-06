namespace SalesManagement.Application.ComplaintResolution.Dto
{
    public class ComplaintResolutionFormDataDto
    {
        // Complaint Info
        public int ComplaintHeaderId { get; set; }
        public string? ComplaintNumber { get; set; }
        public DateOnly? ComplaintDate { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? ComplaintRemarks { get; set; }

        // QC Review Info
        public string? QCStatusName { get; set; }
        public string? SeverityName { get; set; }
        public string? CompensationStructureName { get; set; }
        public string? QCComments { get; set; }
        public DateOnly? ExpectedResolutionDate { get; set; }

        // Feedback Summary
        public int TotalAssignments { get; set; }
        public int SubmittedFeedbacks { get; set; }
        public List<FeedbackSummaryDto>? Feedbacks { get; set; }

        // Complaint Items
        public List<ComplaintItemDto>? Items { get; set; }

        // Existing Resolution (null if no resolution exists yet)
        public ComplaintResolutionDto? ExistingResolution { get; set; }
    }

    public class FeedbackSummaryDto
    {
        public string? RoleName { get; set; }
        public string? ResponsiblePersonName { get; set; }
        public string? RootCauseText { get; set; }
        public string? CorrectiveAction { get; set; }
        public string? PreventiveAction { get; set; }
        public string? FeedbackStatusName { get; set; }
    }

    public class ComplaintItemDto
    {
        public int InvoiceHeaderId { get; set; }
        public string? InvoiceNo { get; set; }
        public DateOnly? InvoiceDate { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public int? LotId { get; set; }
        public string? LotCode { get; set; }
        public int NumberOfPacks { get; set; }
        public decimal NetWeight { get; set; }
        public decimal InvoiceAmount { get; set; }
    }
}
