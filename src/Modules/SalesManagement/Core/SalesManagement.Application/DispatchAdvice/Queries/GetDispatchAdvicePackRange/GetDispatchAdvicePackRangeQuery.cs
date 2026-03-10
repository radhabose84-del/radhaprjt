using MediatR;
using SalesManagement.Application.DispatchAdvice.Dto;

namespace SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdvicePackRange
{
    public class GetDispatchAdvicePackRangeQuery : IRequest<List<DispatchAdvicePackRangeDto>>
    {
        public int ItemId { get; set; }
        public int LotId { get; set; }
        public int PackTypeId { get; set; }
        public int Range { get; set; }
    }
}
