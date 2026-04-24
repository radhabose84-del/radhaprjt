using MediatR;
using SalesManagement.Application.DispatchAdvice.Dto;

namespace SalesManagement.Application.TripSheet.Queries.GetTripSheetDispatchPackingList
{
    public class GetTripSheetDispatchPackingListQuery : IRequest<List<DispatchAdvicePackingListDto>>
    {
        public int TripSheetHeaderId { get; set; }
    }
}
