using MediatR;
using SalesManagement.Application.DispatchAdvice.Dto;

namespace SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdvicePackingList
{
    public class GetDispatchAdvicePackingListQuery : IRequest<DispatchAdvicePackingListDto?>
    {
        public int DispatchAdviceId { get; set; }
    }
}
