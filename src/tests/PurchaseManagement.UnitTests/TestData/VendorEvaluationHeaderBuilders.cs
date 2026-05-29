using PurchaseManagement.Application.VendorEvaluationHeader.Commands.CreateVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Commands.UpdateVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Dto;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.TestData
{
    public static class VendorEvaluationHeaderBuilders
    {
        public static CreateVendorEvaluationHeaderCommand ValidCreateCommand(
            int vendorId = 1,
            int evaluationMonth = 6,
            int evaluationYear = 2026,
            int? gradeId = 1) =>
            new CreateVendorEvaluationHeaderCommand
            {
                VendorId = vendorId,
                EvaluationMonth = evaluationMonth,
                EvaluationYear = evaluationYear,
                EvaluationDate = new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero),
                TotalWeightedScore = 85.5m,
                GradeId = gradeId,
                Remarks = "Monthly evaluation",
                Details = new List<CreateVendorEvaluationDetailItem>
                {
                    new CreateVendorEvaluationDetailItem
                    {
                        CriteriaId = 1,
                        Score = 90m,
                        WeightagePercent = 25m,
                        WeightedScore = 22.5m,
                        ScoringMethod = "Numeric",
                        Remarks = "Good quality"
                    }
                }
            };

        public static UpdateVendorEvaluationHeaderCommand ValidUpdateCommand(
            int id = 1,
            int vendorId = 1,
            int evaluationMonth = 6,
            int evaluationYear = 2026,
            int? gradeId = 1,
            int isActive = 1) =>
            new UpdateVendorEvaluationHeaderCommand
            {
                Id = id,
                VendorId = vendorId,
                EvaluationMonth = evaluationMonth,
                EvaluationYear = evaluationYear,
                EvaluationDate = new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero),
                TotalWeightedScore = 88.0m,
                GradeId = gradeId,
                Remarks = "Updated evaluation",
                IsActive = isActive,
                Details = new List<UpdateVendorEvaluationDetailItem>
                {
                    new UpdateVendorEvaluationDetailItem
                    {
                        CriteriaId = 1,
                        Score = 92m,
                        WeightagePercent = 25m,
                        WeightedScore = 23m,
                        ScoringMethod = "Numeric",
                        Remarks = "Updated quality"
                    }
                }
            };

        public static VendorEvaluationHeaderDto ValidDto(
            int id = 1,
            string? evaluationCode = "EVL001",
            int vendorId = 1,
            string? vendorName = "Test Vendor",
            int? gradeId = 1,
            string? gradeName = "Excellent") =>
            new VendorEvaluationHeaderDto
            {
                Id = id,
                EvaluationCode = evaluationCode,
                VendorId = vendorId,
                VendorName = vendorName,
                EvaluationMonth = 6,
                EvaluationYear = 2026,
                EvaluationDate = new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero),
                TotalWeightedScore = 85.5m,
                GradeId = gradeId,
                GradeName = gradeName,
                Remarks = "Monthly evaluation",
                IsActive = true,
                IsDeleted = false,
                VendorEvaluationDetails = new List<VendorEvaluationDetailDto>
                {
                    new VendorEvaluationDetailDto
                    {
                        Id = 1,
                        VendorEvaluationHeaderId = id,
                        CriteriaId = 1,
                        CriteriaCode = "VEC001",
                        CriteriaName = "Quality",
                        Score = 90m,
                        WeightagePercent = 25m,
                        WeightedScore = 22.5m,
                        ScoringMethod = "Numeric",
                        Remarks = "Good quality"
                    }
                }
            };

        public static PurchaseManagement.Domain.Entities.VendorEvaluation.VendorEvaluationHeader ValidEntity(int id = 1) =>
            new PurchaseManagement.Domain.Entities.VendorEvaluation.VendorEvaluationHeader
            {
                Id = id,
                EvaluationCode = "EVL001",
                VendorId = 1,
                EvaluationMonth = 6,
                EvaluationYear = 2026,
                EvaluationDate = new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero),
                TotalWeightedScore = 85.5m,
                GradeId = 1,
                Remarks = "Monthly evaluation",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
