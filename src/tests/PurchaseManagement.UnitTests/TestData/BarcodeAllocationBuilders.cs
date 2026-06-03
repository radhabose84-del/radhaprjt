using PurchaseManagement.Application.BarcodeAllocation.Command.CreateBarcodeAllocation;
using PurchaseManagement.Application.BarcodeAllocation.Command.UpdateBarcodeAllocation;
using PurchaseManagement.Application.BarcodeAllocation.Dto;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.TestData
{
    public static class BarcodeAllocationBuilders
    {
        public static CreateBarcodeAllocationCommand ValidCreateCommand(
            int seriesId = 1,
            long from = 25000001,
            long to = 25000500,
            string empNo = "1023",
            string empName = "Rajesh Kumar") =>
            new()
            {
                AllocationDate = DateTimeOffset.UtcNow,
                EmployeeNo = empNo,
                EmployeeName = empName,
                BarcodeSeriesId = seriesId,
                BarcodeFrom = from,
                BarcodeTo = to,
                Remarks = "Test allocation"
            };

        public static UpdateBarcodeAllocationCommand ValidUpdateCommand(
            int id = 1,
            int seriesId = 1,
            long from = 25000001,
            long to = 25000500,
            int isActive = 1) =>
            new()
            {
                Id = id,
                AllocationDate = DateTimeOffset.UtcNow,
                EmployeeNo = "1023",
                EmployeeName = "Rajesh Kumar",
                BarcodeSeriesId = seriesId,
                BarcodeFrom = from,
                BarcodeTo = to,
                Remarks = "Updated allocation",
                IsActive = isActive
            };

        public static BarcodeAllocationDto ValidDto(int id = 1, string number = "BBA-2025-0001") =>
            new()
            {
                Id = id,
                AllocationNumber = number,
                AllocationDate = DateTimeOffset.UtcNow,
                EmployeeNo = "1023",
                EmployeeName = "Rajesh Kumar",
                BarcodeSeriesId = 1,
                BarcodeSeriesNumber = "BCS-2025-0001",
                Prefix = "CB",
                BarcodeFrom = 25000001,
                BarcodeTo = 25000500,
                TotalAllocatedQuantity = 500,
                UsedQuantity = 0,
                BalanceQuantity = 500,
                StatusId = 1202,
                Status = "Open",
                Remarks = "Test allocation",
                IsActive = true
            };

        public static IReadOnlyList<BarcodeAllocationLookupDto> ValidLookupList() =>
            new List<BarcodeAllocationLookupDto>
            {
                new() { Id = 1, AllocationNumber = "BBA-2025-0001", EmployeeName = "Rajesh Kumar" }
            };

        public static PurchaseManagement.Domain.Entities.BarcodeAllocation ValidEntity(int id = 1) =>
            new()
            {
                Id = id,
                AllocationNumber = "BBA-2025-0001",
                AllocationDate = DateTimeOffset.UtcNow,
                EmployeeNo = "1023",
                EmployeeName = "Rajesh Kumar",
                BarcodeSeriesId = 1,
                BarcodeFrom = 25000001,
                BarcodeTo = 25000500,
                UsedQuantity = 0,
                StatusId = 1202,
                Remarks = "Test allocation",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
