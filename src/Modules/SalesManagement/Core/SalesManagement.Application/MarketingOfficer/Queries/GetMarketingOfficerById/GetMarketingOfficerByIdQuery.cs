using MediatR;
using SalesManagement.Application.MarketingOfficer.Dto;

namespace SalesManagement.Application.MarketingOfficer.Queries.GetMarketingOfficerById
{
    public class GetMarketingOfficerByIdQuery : IRequest<MarketingOfficerDto?>
    {
        public int Id { get; set; }
    }
}
