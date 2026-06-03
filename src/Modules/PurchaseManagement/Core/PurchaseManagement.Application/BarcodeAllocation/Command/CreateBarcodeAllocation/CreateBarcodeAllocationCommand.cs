using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.BarcodeAllocation.Command.CreateBarcodeAllocation
{
    public class CreateBarcodeAllocationCommand : IRequest<ApiResponseDTO<int>>
    {
        public DateTimeOffset AllocationDate { get; set; }
        public string? EmployeeNo { get; set; }
        public string? EmployeeName { get; set; }
        public int BarcodeSeriesId { get; set; }
        public long BarcodeFrom { get; set; }
        public long BarcodeTo { get; set; }
        public string? Remarks { get; set; }
    }
}
