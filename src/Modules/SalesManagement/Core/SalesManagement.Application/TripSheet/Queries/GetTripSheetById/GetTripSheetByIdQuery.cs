using MediatR;
using SalesManagement.Application.TripSheet.Dto;

namespace SalesManagement.Application.TripSheet.Queries.GetTripSheetById
{
    public class GetTripSheetByIdQuery : IRequest<TripSheetHeaderDto?>
    {
        public int Id { get; set; }
    }
}
