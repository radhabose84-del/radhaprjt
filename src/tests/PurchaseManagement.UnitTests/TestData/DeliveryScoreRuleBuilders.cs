using PurchaseManagement.Application.DeliveryScoreRule.Commands.CreateDeliveryScoreRule;
using PurchaseManagement.Application.DeliveryScoreRule.Commands.UpdateDeliveryScoreRule;
using PurchaseManagement.Application.DeliveryScoreRule.Dto;
using Contracts.Dtos.Lookups.Purchase;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.TestData
{
    public static class DeliveryScoreRuleBuilders
    {
        public static CreateDeliveryScoreRuleCommand ValidCreateCommand(
            string? ruleCode = "DSR001",
            string? description = "On-time delivery",
            int delayDaysFrom = 0,
            int delayDaysTo = 0,
            decimal score = 100m,
            int sortOrder = 1) =>
            new CreateDeliveryScoreRuleCommand
            {
                RuleCode = ruleCode,
                Description = description,
                DelayDaysFrom = delayDaysFrom,
                DelayDaysTo = delayDaysTo,
                Score = score,
                SortOrder = sortOrder
            };

        public static UpdateDeliveryScoreRuleCommand ValidUpdateCommand(
            int id = 1,
            string? description = "Updated on-time delivery",
            int delayDaysFrom = 0,
            int delayDaysTo = 3,
            decimal score = 90m,
            int sortOrder = 1,
            int isActive = 1) =>
            new UpdateDeliveryScoreRuleCommand
            {
                Id = id,
                Description = description,
                DelayDaysFrom = delayDaysFrom,
                DelayDaysTo = delayDaysTo,
                Score = score,
                SortOrder = sortOrder,
                IsActive = isActive
            };

        public static DeliveryScoreRuleDto ValidDto(
            int id = 1,
            string? ruleCode = "DSR001",
            string? description = "On-time delivery") =>
            new DeliveryScoreRuleDto
            {
                Id = id,
                RuleCode = ruleCode,
                Description = description,
                DelayDaysFrom = 0,
                DelayDaysTo = 0,
                Score = 100m,
                SortOrder = 1,
                IsActive = true,
                IsDeleted = false
            };

        public static IReadOnlyList<DeliveryScoreRuleLookupDto> ValidLookupList() =>
            new List<DeliveryScoreRuleLookupDto>
            {
                new DeliveryScoreRuleLookupDto { Id = 1, RuleCode = "DSR001", Description = "On-time delivery" }
            };

        public static PurchaseManagement.Domain.Entities.VendorEvaluation.DeliveryScoreRule ValidEntity(int id = 1) =>
            new PurchaseManagement.Domain.Entities.VendorEvaluation.DeliveryScoreRule
            {
                Id = id,
                RuleCode = "DSR001",
                Description = "On-time delivery",
                DelayDaysFrom = 0,
                DelayDaysTo = 0,
                Score = 100m,
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
