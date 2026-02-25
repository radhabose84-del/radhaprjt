using MediatR;

namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGateEntryPending
{
    public class GetGateEntryPendingQuery : IRequest<List<GetGateEntryPendingDto>>
    {
        public int PartyId { get; set; }
        public int PoId { get; set; }
        
    }
}