using Contracts.Common;
using MediatR;
using SalesManagement.Application.CustomerVisit.Dto;

namespace SalesManagement.Application.CustomerVisit.Commands.CreateCustomerVisit
{
    public class CreateCustomerVisitCommand : IRequest<ApiResponseDTO<int>>
    {
        public int CustomerId { get; set; }
        public int VisitTypeId { get; set; }
        public DateTimeOffset VisitDateTime { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? ImageName { get; set; }
        public string? Remarks { get; set; }
        public int MarketingOfficerId { get; set; }

        // Detail list
        public List<CreateCustomerVisitProductDto>? Products { get; set; }
    }
}
