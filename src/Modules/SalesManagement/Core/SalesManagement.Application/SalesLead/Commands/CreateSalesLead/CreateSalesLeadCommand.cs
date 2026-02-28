using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesLead.Commands.CreateSalesLead
{
    public class CreateSalesLeadCommand : IRequest<ApiResponseDTO<int>>
    {
        public int? PartyId { get; set; }
        public string? ProspectCompanyName { get; set; }
        public int? CityId { get; set; }
        public string? ContactName { get; set; }
        public string? MobileNumber { get; set; }
        public string? EmailId { get; set; }
        public int? ContactId { get; set; }
        public int? ItemId { get; set; }
        public decimal? RequirementQty { get; set; }
        public DateOnly? ExpectedDate { get; set; }
        public string? Remarks { get; set; }
        public int? LeadSourceId { get; set; }
        public int MarketingOfficerId { get; set; }
        public DateTimeOffset InteractionDate { get; set; }
    }
}
