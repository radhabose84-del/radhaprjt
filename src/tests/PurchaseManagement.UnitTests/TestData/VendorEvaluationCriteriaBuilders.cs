using PurchaseManagement.Application.VendorEvaluationCriteria.Commands.CreateVendorEvaluationCriteria;
using PurchaseManagement.Application.VendorEvaluationCriteria.Commands.UpdateVendorEvaluationCriteria;
using PurchaseManagement.Application.VendorEvaluationCriteria.Dto;
using Contracts.Dtos.Lookups.Purchase;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.TestData
{
    public static class VendorEvaluationCriteriaBuilders
    {
        public static CreateVendorEvaluationCriteriaCommand ValidCreateCommand(
            string? criteriaCode = "VEC001",
            string? criteriaName = "Quality",
            string? description = "Quality assessment criteria",
            decimal weightagePercent = 25m,
            int scoringMethodId = 1,
            decimal minimumScore = 0m,
            int ratingImpactId = 1,
            int sortOrder = 1) =>
            new CreateVendorEvaluationCriteriaCommand
            {
                CriteriaCode = criteriaCode,
                CriteriaName = criteriaName,
                Description = description,
                WeightagePercent = weightagePercent,
                ScoringMethodId = scoringMethodId,
                MinimumScore = minimumScore,
                RatingImpactId = ratingImpactId,
                SortOrder = sortOrder
            };

        public static UpdateVendorEvaluationCriteriaCommand ValidUpdateCommand(
            int id = 1,
            string? criteriaName = "Updated Quality",
            string? description = "Updated description",
            decimal weightagePercent = 30m,
            int scoringMethodId = 1,
            decimal minimumScore = 5m,
            int ratingImpactId = 1,
            int sortOrder = 2,
            int isActive = 1) =>
            new UpdateVendorEvaluationCriteriaCommand
            {
                Id = id,
                CriteriaName = criteriaName,
                Description = description,
                WeightagePercent = weightagePercent,
                ScoringMethodId = scoringMethodId,
                MinimumScore = minimumScore,
                RatingImpactId = ratingImpactId,
                SortOrder = sortOrder,
                IsActive = isActive
            };

        public static VendorEvaluationCriteriaDto ValidDto(
            int id = 1,
            string? criteriaCode = "VEC001",
            string? criteriaName = "Quality",
            int scoringMethodId = 1,
            string? scoringMethodName = "Numeric",
            int ratingImpactId = 1,
            string? ratingImpactName = "High") =>
            new VendorEvaluationCriteriaDto
            {
                Id = id,
                CriteriaCode = criteriaCode,
                CriteriaName = criteriaName,
                Description = "Quality assessment criteria",
                WeightagePercent = 25m,
                ScoringMethodId = scoringMethodId,
                ScoringMethodName = scoringMethodName,
                MinimumScore = 0m,
                RatingImpactId = ratingImpactId,
                RatingImpactName = ratingImpactName,
                SortOrder = 1,
                IsActive = true,
                IsDeleted = false
            };

        public static IReadOnlyList<VendorEvaluationCriteriaLookupDto> ValidLookupList() =>
            new List<VendorEvaluationCriteriaLookupDto>
            {
                new VendorEvaluationCriteriaLookupDto { Id = 1, CriteriaCode = "VEC001", CriteriaName = "Quality" }
            };

        public static PurchaseManagement.Domain.Entities.VendorEvaluation.VendorEvaluationCriteria ValidEntity(int id = 1) =>
            new PurchaseManagement.Domain.Entities.VendorEvaluation.VendorEvaluationCriteria
            {
                Id = id,
                CriteriaCode = "VEC001",
                CriteriaName = "Quality",
                Description = "Quality assessment criteria",
                WeightagePercent = 25m,
                ScoringMethodId = 1,
                MinimumScore = 0m,
                RatingImpactId = 1,
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
