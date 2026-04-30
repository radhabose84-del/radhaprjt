using MediatR;
using SalesManagement.Application.DispatchAdvice.Dto;

namespace SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdvicePackingListByIds
{
    public class GetDispatchAdvicePackingListByIdsQuery : IRequest<List<DispatchAdvicePackingListDto>>
    {
        public List<int> DispatchAdviceIds { get; set; } = [];
    }
}
