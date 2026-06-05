using Contracts.Dtos.Lookups.Inventory;
using PurchaseManagement.Application.RawMaterialPO.Commands.CreateRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Commands.UpdateRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Dto;
using PurchaseManagement.Domain.Entities.RawMaterialPO;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.TestData
{
    public static class RawMaterialPOBuilders
    {
        private static readonly DateTimeOffset SampleDate = new(2026, 6, 4, 0, 0, 0, TimeSpan.Zero);

        public static CreateRawMaterialPOCommand ValidCreateCommand(
            int ocrId = 1, int docTypeId = 1, decimal quantity = 500m, decimal rate = 68500m)
        {
            var itemValue = quantity * rate;
            var cgst = itemValue * 0.025m;
            var totalGst = cgst * 2;
            return new()
            {
                OcrId = ocrId,
                PODate = SampleDate,
                ProcurementDocumentTypeId = docTypeId,
                Remarks = "Converted from OCR",
                TaxableTotal = itemValue,
                TotalGstAmount = totalGst,
                NetTotal = itemValue + totalGst,
                Details = new List<CreateRawMaterialPODetailDto>
                {
                    new()
                    {
                        ItemId = 1, HsnId = 1, Quantity = quantity, Weight = 85000m, Rate = rate,
                        ItemValue = itemValue, CGSTPercentage = 2.5m, SGSTPercentage = 2.5m, IGSTPercentage = 5m,
                        CGSTValue = cgst, SGSTValue = cgst, IGSTValue = 0m, TotalGST = totalGst, NetValue = itemValue + totalGst
                    }
                }
            };
        }

        public static UpdateRawMaterialPOCommand ValidUpdateCommand(
            int id = 1, int docTypeId = 1, int isActive = 1, decimal quantity = 600m)
        {
            var itemValue = quantity * 68500m;
            var cgst = itemValue * 0.025m;
            var totalGst = cgst * 2;
            return new()
            {
                Id = id,
                PODate = SampleDate,
                ProcurementDocumentTypeId = docTypeId,
                Remarks = "Updated",
                IsActive = isActive,
                TaxableTotal = itemValue,
                TotalGstAmount = totalGst,
                NetTotal = itemValue + totalGst,
                Details = new List<UpdateRawMaterialPODetailDto>
                {
                    new()
                    {
                        ItemId = 1, HsnId = 1, Quantity = quantity, Weight = 90000m, Rate = 68500m,
                        ItemValue = itemValue, CGSTPercentage = 2.5m, SGSTPercentage = 2.5m, IGSTPercentage = 5m,
                        CGSTValue = cgst, SGSTValue = cgst, IGSTValue = 0m, TotalGST = totalGst, NetValue = itemValue + totalGst
                    }
                }
            };
        }

        public static RawMaterialPODto ValidDto(int id = 1, int ocrId = 1) =>
            new()
            {
                Id = id,
                UnitId = 1,
                PONumber = "RMPO-2026-0001",
                PODate = SampleDate,
                OcrId = ocrId,
                OcrNumber = "OCR-2025-0004",
                ProcurementDocumentTypeId = 1,
                ProcurementDocumentTypeName = "Purchase Order",
                StatusId = 1,
                StatusName = "Partially Converted",
                TaxableTotal = 34_250_000m,
                TotalGstAmount = 1_712_500m,
                NetTotal = 35_962_500m,
                IsActive = true,
                IsDeleted = false,
                Details = new List<RawMaterialPODetailDto>
                {
                    new() { Id = 1, POHeaderId = id, ItemId = 1, HsnId = 1, Quantity = 500m, Rate = 68500m, ItemValue = 34_250_000m, TotalGST = 1_712_500m, NetValue = 35_962_500m }
                }
            };

        public static RawMaterialPOHeader ValidEntity(int id = 1, int ocrId = 1) =>
            new()
            {
                Id = id,
                UnitId = 1,
                PONumber = "RMPO-2026-0001",
                PODate = SampleDate,
                OcrId = ocrId,
                ProcurementDocumentTypeId = 1,
                StatusId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                RawMaterialPODetails = new List<RawMaterialPODetail>
                {
                    new() { Id = 1, ItemId = 1, HsnId = 1, Quantity = 500m, Rate = 68500m }
                }
            };

        public static IReadOnlyList<RawMaterialPOLookupDto> ValidLookupList() =>
            new List<RawMaterialPOLookupDto> { new() { Id = 1, PONumber = "RMPO-2026-0001" } };

        // HSN with split GST (CGST 2.5 + SGST 2.5 + IGST 5 — IGST unused for intra-state)
        public static IReadOnlyList<HSNLookupDto> HsnList(int hsnId = 1) =>
            new List<HSNLookupDto>
            {
                new() { Id = hsnId, HSNCode = "5201", GSTPercentage = 5m, CGSTPercentage = 2.5m, SGSTPercentage = 2.5m, IGSTPercentage = 5m, IsActive = true }
            };
    }
}
