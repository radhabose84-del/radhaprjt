using MediatR;

namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGateEntryPendingPo
{
    public class GetGateEntryPendingPoQuery : IRequest<List<GetGateEntryPendingPoDto>>
    {
        public int PartyId { get; set; }
    }
}