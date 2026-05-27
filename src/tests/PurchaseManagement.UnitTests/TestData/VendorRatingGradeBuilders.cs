using PurchaseManagement.Application.VendorRatingGrade.Commands.CreateVendorRatingGrade;
using PurchaseManagement.Application.VendorRatingGrade.Commands.UpdateVendorRatingGrade;
using PurchaseManagement.Application.VendorRatingGrade.Dto;
using Contracts.Dtos.Lookups.Purchase;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.TestData
{
    public static class VendorRatingGradeBuilders
    {
        public static CreateVendorRatingGradeCommand ValidCreateCommand(
            string? gradeCode = "GRD001",
            string? gradeName = "Excellent",
            decimal minScore = 90m,
            decimal maxScore = 100m,
            string? actionDescription = "Preferred vendor",
            int? actionTypeId = 1,
            int sortOrder = 1) =>
            new CreateVendorRatingGradeCommand
            {
                GradeCode = gradeCode,
                GradeName = gradeName,
                MinScore = minScore,
                MaxScore = maxScore,
                ActionDescription = actionDescription,
                ActionTypeId = actionTypeId,
                SortOrder = sortOrder
            };

        public static UpdateVendorRatingGradeCommand ValidUpdateCommand(
            int id = 1,
            string? gradeName = "Updated Excellent",
            decimal minScore = 85m,
            decimal maxScore = 100m,
            string? actionDescription = "Updated preferred vendor",
            int? actionTypeId = 1,
            int sortOrder = 1,
            int isActive = 1) =>
            new UpdateVendorRatingGradeCommand
            {
                Id = id,
                GradeName = gradeName,
                MinScore = minScore,
                MaxScore = maxScore,
                ActionDescription = actionDescription,
                ActionTypeId = actionTypeId,
                SortOrder = sortOrder,
                IsActive = isActive
            };

        public static VendorRatingGradeDto ValidDto(
            int id = 1,
            string? gradeCode = "GRD001",
            string? gradeName = "Excellent",
            int? actionTypeId = 1,
            string? actionTypeName = "Approve") =>
            new VendorRatingGradeDto
            {
                Id = id,
                GradeCode = gradeCode,
                GradeName = gradeName,
                MinScore = 90m,
                MaxScore = 100m,
                ActionDescription = "Preferred vendor",
                ActionTypeId = actionTypeId,
                ActionTypeName = actionTypeName,
                SortOrder = 1,
                IsActive = true,
                IsDeleted = false
            };

        public static IReadOnlyList<VendorRatingGradeLookupDto> ValidLookupList() =>
            new List<VendorRatingGradeLookupDto>
            {
                new VendorRatingGradeLookupDto { Id = 1, GradeCode = "GRD001", GradeName = "Excellent" }
            };

        public static PurchaseManagement.Domain.Entities.VendorEvaluation.VendorRatingGrade ValidEntity(int id = 1) =>
            new PurchaseManagement.Domain.Entities.VendorEvaluation.VendorRatingGrade
            {
                Id = id,
                GradeCode = "GRD001",
                GradeName = "Excellent",
                MinScore = 90m,
                MaxScore = 100m,
                ActionDescription = "Preferred vendor",
                ActionTypeId = 1,
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
