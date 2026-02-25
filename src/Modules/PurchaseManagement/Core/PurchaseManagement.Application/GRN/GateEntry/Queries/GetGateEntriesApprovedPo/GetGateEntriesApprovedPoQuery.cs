using MediatR;

namespace PurchaseManagement.Application.GRN.GateEntry.Queries.GetGateEntriesApprovedPo
{
    public class GetGateEntriesApprovedPoQuery : IRequest<List<GetGateEntriesApprovedPoDto>>
    {
        public int PartyId { get; set; }
    }
}