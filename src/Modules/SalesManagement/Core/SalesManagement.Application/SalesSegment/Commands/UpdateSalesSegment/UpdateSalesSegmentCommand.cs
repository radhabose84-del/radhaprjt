
using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesSegment.Commands.UpdateSalesSegment
{
    public class UpdateSalesSegmentCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }

        // Note: Composite key fields (SalesOrganisationId, SalesChannelId, BusinessUnitId) are IMMUTABLE
        // They cannot be changed after creation

        // Mutable fields
        public int? CurrencyId { get; set; }
        public DateTime? ValidFrom { get; set; }
        public string? SegmentName { get; set; }
        public int IsActive { get; set; }  // 1=Active, 0=Inactive
    }
}
