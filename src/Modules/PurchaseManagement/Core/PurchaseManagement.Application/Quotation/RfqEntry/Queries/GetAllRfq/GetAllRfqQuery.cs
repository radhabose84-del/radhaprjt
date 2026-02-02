using PurchaseManagement.Application.Quotation.RfqEntry.DTOs;
using MediatR;

namespace PurchaseManagement.Application.Quotation.RfqEntry.Queries.GetAllRfq
{
    public class GetAllRfqQuery 
        : IRequest<(IReadOnlyList<RfqListItemDto> Items, int Total)>
    {
        public int? StatusId   { get; set; }
        public int  PageNumber { get; set; } = 1;
        public int  PageSize   { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}
