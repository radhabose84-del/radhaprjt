using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Purchase;
using QCManagement.Application.QcInspection.Commands.CreateQcInspection;
using QCManagement.Application.QcInspection.Commands.SaveDisposition;
using QCManagement.Application.QcInspection.Commands.SaveParameterCollection;
using QCManagement.Application.QcInspection.Dto;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.UnitTests.TestData
{
    public static class QcInspectionBuilders
    {
        public static CreateQcInspectionCommand ValidCreateCommand(int grnDetailId = 4321) =>
            new() { GrnDetailId = grnDetailId };

        public static GrnLookupDto ValidGrnLookup(int grnDetailId = 4321, int grnHeaderId = 100, int itemId = 7) =>
            new()
            {
                GrnHeaderId = grnHeaderId,
                GrnDetailId = grnDetailId,
                GrnNo = "GRN-789012",
                GrnDate = DateTimeOffset.UtcNow,
                SupplierId = 50,
                InvoiceNo = "INV-1",
                ItemId = itemId,
                BatchNumber = "BN-1",
                ReceivedQuantity = 1000m,
                ReceivedUomId = 3
            };

        public static ItemLookupDto ValidItemLookup(int itemId = 7, bool inspectionRequired = true, int categoryId = 9) =>
            new()
            {
                Id = itemId,
                ItemCode = "ITM-1",
                ItemName = "Cotton Yarn",
                ItemCategoryId = categoryId,
                InspectionRequired = inspectionRequired
            };

        public static QcSpecSnapshotDto ValidSnapshot(int specId = 5) =>
            new()
            {
                QualitySpecificationId = specId,
                QualitySpecificationCode = "QS-0001",
                QualityTemplateId = 11,
                QualityTemplateCode = "QT-000001",
                QcTypeId = 8,
                Parameters = new List<QcSpecParamSnapshotDto>
                {
                    new()
                    {
                        QualitySpecificationParameterId = 501,
                        QualityParameterId = 1,
                        ParameterCode = "QP-1",
                        ParameterName = "Tensile",
                        DataTypeId = 2,
                        ValidationTypeId = 3,
                        ValidationTypeCode = "RNG",
                        MinValue = 10m,
                        MaxValue = 50m,
                        SeverityId = 6,
                        SeverityCode = "CRT",
                        FailureActionId = 7,
                        SortOrder = 1
                    }
                }
            };

        public static SaveParameterCollectionCommand ValidParamsCommand(int hdrId = 88) =>
            new()
            {
                QcInspectionHdrId = hdrId,
                Parameters = new List<ParameterResultInputDto>
                {
                    new() { DetailId = 501, ActualValue = "40", Remarks = "ok" }
                }
            };

        public static SaveDispositionCommand ValidDispositionCommand(
            int hdrId = 88, string code = "APR", decimal acc = 1000m, decimal rej = 0m, string? remarks = null) =>
            new()
            {
                QcInspectionHdrId = hdrId,
                QcStatusCode = code,
                AcceptedQuantity = acc,
                RejectedQuantity = rej,
                DispositionRemarks = remarks
            };

        public static QcDispositionContextDto ValidContext(int hdrId = 88) =>
            new()
            {
                GrnHeaderId = 100,
                GrnDetailId = 4321,
                ReceivedQuantity = 1000m,
                ReceivedUomId = 3,
                QcInspectionNo = "QCI-2026-00001"
            };

        public static QcInspectionListDto ValidListDto(int id = 1) =>
            new()
            {
                InspectionId = id,
                QcInspectionNo = "QCI-2026-00001",
                GrnHeaderId = 100,
                GrnDetailId = 4321,
                ReceivedQuantity = 1000m,
                QcStatusCode = "APR",
                QcStatusName = "Approved",
                InspectionDate = DateTimeOffset.UtcNow
            };

        public static QcInspectionDto ValidDto(int id = 88) =>
            new()
            {
                Id = id,
                QcInspectionNo = "QCI-2026-00001",
                GrnHeaderId = 100,
                GrnDetailId = 4321,
                QualitySpecificationId = 5,
                ReceivedQuantity = 1000m,
                InspectionDate = DateTimeOffset.UtcNow,
                IsActive = true
            };

        public static QCManagement.Domain.Entities.QcInspectionHdr ValidEntity(int id = 88) =>
            new()
            {
                Id = id,
                QcInspectionNo = "QCI-2026-00001",
                InspectionDate = DateTimeOffset.UtcNow,
                GrnHeaderId = 100,
                GrnDetailId = 4321,
                QualitySpecificationId = 5,
                QualitySpecificationCode = "QS-0001",
                QualityTemplateId = 11,
                QualityTemplateCode = "QT-000001",
                QcTypeId = 8,
                InspectorUserId = 1,
                InspectorName = "tester",
                ReceivedQuantity = 1000m,
                ReceivedUomId = 3,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
