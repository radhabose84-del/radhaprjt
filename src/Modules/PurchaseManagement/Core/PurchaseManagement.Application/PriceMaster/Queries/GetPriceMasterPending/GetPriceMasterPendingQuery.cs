using MediatR;

namespace PurchaseManagement.Application.PriceMaster.Queries.GetPriceMasterPending
{
    public sealed class GetPriceMasterPendingQuery 
        : IRequest<(List<PriceMasterPendingGroupDto> Items, int TotalCount)>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize  { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}
