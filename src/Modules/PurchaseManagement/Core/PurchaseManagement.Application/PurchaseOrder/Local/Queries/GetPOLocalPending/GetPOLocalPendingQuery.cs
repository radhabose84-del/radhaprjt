using MediatR;

namespace   PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPOLocalPending
{
    public sealed class GetPOLocalPendingQuery
        : IRequest<(List<GetPOLocalPendingGroupDto> Items, int TotalCount)>
    {
        public int? PageNumber { get; set; } = 1;
        public int? PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
        public int? PoId { get; set; }         
        public int? PoMethodId { get; set; }    
    }
}

    