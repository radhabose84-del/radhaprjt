
using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesSegment.Commands.CreateSalesSegment
{
    public class CreateSalesSegmentCommand : IRequest<ApiResponseDTO<int>>
    {
        public int SalesOrganisationId { get; set; }
        public int SalesChannelId { get; set; }
        public int BusinessUnitId { get; set; }
        public int? CurrencyId { get; set; }
        public DateTime? ValidFrom { get; set; }
        public string SegmentName { get; set; } = null!;
    }
}
