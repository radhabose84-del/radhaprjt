using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.TripSheet.Commands.CreateTripSheet
{
    public class CreateTripSheetCommand : IRequest<ApiResponseDTO<int>>
    {
        public DateOnly TripDate { get; set; }
        public string? VehicleNo { get; set; }
        public string? Remarks { get; set; }
        public List<CreateTripSheetDetailItem>? Details { get; set; }
    }

    public class CreateTripSheetDetailItem
    {
        public int DispatchAdviceHeaderId { get; set; }
        public int SequenceNo { get; set; }
    }
}
