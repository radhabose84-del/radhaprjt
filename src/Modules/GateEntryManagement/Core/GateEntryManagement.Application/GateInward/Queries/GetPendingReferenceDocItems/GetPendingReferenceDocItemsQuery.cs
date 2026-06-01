using Contracts.Common;
using Contracts.Dtos.Common;
using MediatR;

namespace GateEntryManagement.Application.GateInward.Queries.GetPendingReferenceDocItems
{
    /// <summary>
    /// Loads pending line items for a user's multi-select PO picks on the centralized
    /// Gate Inward Entry screen. Response groups items per PO and preserves the input
    /// order of <see cref="PoIds"/>.
    /// </summary>
    public class GetPendingReferenceDocItemsQuery
        : IRequest<ApiResponseDTO<List<PendingReferenceDocLineDto>>>
    {
        public int PartyId { get; set; }
        public int ReferenceDocumentTypeId { get; set; }
        public List<int> PoIds { get; set; } = new();
    }
}
