using MediatR;
using PurchaseManagement.Application.PriceMaster.Dtos;
using PurchaseManagement.Application.Common;

namespace PurchaseManagement.Application.PriceMaster.Queries.GetAll
{
    public sealed class GetAllPriceMasterQuery : IRequest<PagedResult<PriceMasterGetAllDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
        public int? ItemId { get; set; }
        public decimal? QtyFrom { get; set; }
        public decimal? QtyTo { get; set; }
        public int? statusId { get; set; }
        public bool expiredList { get; set; }=false;
     }
}
