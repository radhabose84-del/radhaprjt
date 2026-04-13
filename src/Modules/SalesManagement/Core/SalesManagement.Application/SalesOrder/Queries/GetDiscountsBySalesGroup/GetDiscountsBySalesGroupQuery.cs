using MediatR;

namespace SalesManagement.Application.SalesOrder.Queries.GetDiscountsBySalesGroup
{
    public class GetDiscountsBySalesGroupQuery : IRequest<List<DiscountsBySalesGroupDto>>
    {
        public int SalesGroupId { get; set; }
        public int SlabTypeId { get; set; }
        public int PaymentTermId { get; set; }
    }
}
