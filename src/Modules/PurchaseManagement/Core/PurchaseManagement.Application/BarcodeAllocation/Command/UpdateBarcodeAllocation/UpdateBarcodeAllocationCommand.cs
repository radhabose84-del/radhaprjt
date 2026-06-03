using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.BarcodeAllocation.Command.UpdateBarcodeAllocation
{
    public class UpdateBarcodeAllocationCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public DateTimeOffset AllocationDate { get; set; }
        public string? EmployeeNo { get; set; }
        public string? EmployeeName { get; set; }
        public int BarcodeSeriesId { get; set; }
        public long BarcodeFrom { get; set; }
        public long BarcodeTo { get; set; }
        public string? Remarks { get; set; }
        public int IsActive { get; set; }
    }
}
