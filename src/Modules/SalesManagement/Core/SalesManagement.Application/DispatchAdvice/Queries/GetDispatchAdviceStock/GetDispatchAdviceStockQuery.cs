using MediatR;
using SalesManagement.Application.DispatchAdvice.Dto;

namespace SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdviceStock
{
    public class GetDispatchAdviceStockQuery : IRequest<List<DispatchAdviceStockDto>>
    {
        public int ItemId { get; set; }
        public int LotId { get; set; }
    }
}
