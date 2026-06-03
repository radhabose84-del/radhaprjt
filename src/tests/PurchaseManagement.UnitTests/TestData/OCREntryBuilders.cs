using PurchaseManagement.Application.OCREntry.Commands.CreateOCREntry;
using PurchaseManagement.Application.OCREntry.Commands.UpdateOCREntry;
using PurchaseManagement.Application.OCREntry.Dto;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.TestData
{
    public static class OCREntryBuilders
    {
        public static CreateOCREntryCommand ValidCreateCommand() => new()
        {
            OcrDate = DateTimeOffset.UtcNow,
            ProcurementSourceId = 1,
            ProcurementTypeId = 2,
            BrokerDirectId = 3,
            GradeId = 4,
            PaymentTermId = 5,
            SupplierId = 10,
            LocationId = 11,
            StationId = 12,
            ItemId = 13,
            CountId = 14,
            Quantity = 100m,
            Weight = 5000m,
            Rate = 75000m,
            ExpectedDispatchDate = DateTimeOffset.UtcNow.AddDays(7),
            DocumentPath = "docs/ocr.png"
        };

        public static UpdateOCREntryCommand ValidUpdateCommand(int id = 1) => new()
        {
            Id = id,
            OcrDate = DateTimeOffset.UtcNow,
            ProcurementSourceId = 1,
            ProcurementTypeId = 2,
            BrokerDirectId = 3,
            GradeId = 4,
            PaymentTermId = 5,
            SupplierId = 10,
            LocationId = 11,
            StationId = 12,
            ItemId = 13,
            CountId = 14,
            Quantity = 120m,
            Weight = 5200m,
            Rate = 76000m,
            ExpectedDispatchDate = DateTimeOffset.UtcNow.AddDays(7),
            DocumentPath = "docs/ocr.png",
            IsActive = 1
        };

        public static OCREntryDto ValidDto(int id = 1) => new()
        {
            Id = id,
            OcrNumber = "OCR-2026-0001",
            OcrDate = DateTimeOffset.UtcNow,
            ProcurementSourceId = 1,
            ProcurementSourceName = "Agent/Broker",
            ProcurementTypeId = 2,
            ProcurementTypeName = "Normal Purchase Order",
            BrokerDirectId = 3,
            BrokerDirectName = "Direct",
            PaymentTermId = 5,
            PaymentTermName = "30 Days",
            StatusId = 9,
            StatusName = "Pending",
            SupplierId = 10,
            SupplierName = "Test Ginner",
            LocationId = 11,
            LocationName = "Coimbatore",
            StationId = 12,
            StationName = "CBE Station",
            ItemId = 13,
            ItemName = "Cotton DCH-32",
            CountId = 14,
            CountName = "30s",
            Quantity = 100m,
            Rate = 75000m,
            IsActive = true,
            IsDeleted = false
        };

        public static PurchaseManagement.Domain.Entities.OCREntry ValidEntity(int id = 1) => new()
        {
            Id = id,
            OcrNumber = "OCR-2026-0001",
            OcrDate = DateTimeOffset.UtcNow,
            ProcurementSourceId = 1,
            ProcurementTypeId = 2,
            BrokerDirectId = 3,
            GradeId = 4,
            PaymentTermId = 5,
            SupplierId = 10,
            LocationId = 11,
            StationId = 12,
            ItemId = 13,
            CountId = 14,
            Quantity = 100m,
            Rate = 75000m,
            StatusId = 9,
            IsActive = Status.Active,
            IsDeleted = IsDelete.NotDeleted
        };
    }
}
