using PurchaseManagement.Application.BarcodeSeries.Command.CreateBarcodeSeries;
using PurchaseManagement.Application.BarcodeSeries.Command.UpdateBarcodeSeries;
using PurchaseManagement.Application.BarcodeSeries.Dto;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.TestData
{
    public static class BarcodeSeriesBuilders
    {
        public static CreateBarcodeSeriesCommand ValidCreateCommand(
            int prefixId = 1,
            long start = 25000001,
            long end = 25005000,
            string? remarks = "Test series") =>
            new()
            {
                PrefixId = prefixId,
                BarcodeStartNumber = start,
                BarcodeEndNumber = end,
                GenerationDate = DateTimeOffset.UtcNow,
                Remarks = remarks
            };

        public static UpdateBarcodeSeriesCommand ValidUpdateCommand(
            int id = 1,
            int prefixId = 1,
            long start = 25000001,
            long end = 25005000,
            int isActive = 1) =>
            new()
            {
                Id = id,
                PrefixId = prefixId,
                BarcodeStartNumber = start,
                BarcodeEndNumber = end,
                Remarks = "Updated series",
                IsActive = isActive
            };

        public static BarcodeSeriesDto ValidDto(
            int id = 1,
            string number = "BCS-2025-0001",
            int prefixId = 1,
            string prefix = "CB") =>
            new()
            {
                Id = id,
                BarcodeSeriesNumber = number,
                PrefixId = prefixId,
                Prefix = prefix,
                GenerationDate = DateTimeOffset.UtcNow,
                BarcodeStartNumber = 25000001,
                BarcodeEndNumber = 25005000,
                BarcodeFormatPreview = "CB25000001",
                TotalBarcodeCount = 5000,
                AllocatedCount = 0,
                Balance = 5000,
                StatusId = 10,
                Status = "Open",
                Remarks = "Test series",
                IsActive = true
            };

        public static IReadOnlyList<BarcodeSeriesLookupDto> ValidLookupList() =>
            new List<BarcodeSeriesLookupDto>
            {
                new() { Id = 1, BarcodeSeriesNumber = "BCS-2025-0001", Prefix = "CB" }
            };

        public static PurchaseManagement.Domain.Entities.BarcodeSeries ValidEntity(int id = 1) =>
            new()
            {
                Id = id,
                BarcodeSeriesNumber = "BCS-2025-0001",
                PrefixId = 1,
                BarcodeStartNumber = 25000001,
                BarcodeEndNumber = 25005000,
                GenerationDate = DateTimeOffset.UtcNow,
                AllocatedCount = 0,
                StatusId = 10,
                Remarks = "Test series",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
