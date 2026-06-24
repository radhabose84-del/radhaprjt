using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.JournalThresholdRule.Commands.CreateJournalThresholdRule;
using FinanceManagement.Application.JournalMaster.JournalThresholdRule.Commands.UpdateJournalThresholdRule;

namespace FinanceManagement.UnitTests.TestData
{
    internal static class JournalThresholdRuleBuilders
    {
        public static CreateJournalThresholdRuleCommand ValidCreateCommand(int ruleTypeId = 131, decimal? threshold = 5000000m) =>
            new()
            {
                RuleTypeId = ruleTypeId,
                ThresholdValue = threshold,
                Active = true,
                EffectiveFrom = new DateOnly(2026, 4, 1)
            };

        public static UpdateJournalThresholdRuleCommand ValidUpdateCommand(int id = 1, int isActive = 1) =>
            new()
            {
                Id = id,
                RuleTypeId = 131,
                ThresholdValue = 6000000m,
                Active = false,
                EffectiveFrom = new DateOnly(2026, 4, 1),
                IsActive = isActive
            };

        public static JournalThresholdRuleDto ValidDto(int id = 1) =>
            new()
            {
                Id = id,
                RuleTypeId = 131,
                RuleTypeName = "Amount Over",
                ThresholdValue = 5000000m,
                Active = true,
                EffectiveFrom = new DateOnly(2026, 4, 1),
                IsActive = true,
                IsDeleted = false
            };

        public static List<JournalThresholdRuleLookupDto> ValidLookupList() =>
            new() { new JournalThresholdRuleLookupDto { Id = 1, RuleTypeId = 131, RuleTypeName = "Amount Over" } };

        public static JournalFlagDto ValidFlagDto(int id = 1) =>
            new()
            {
                Id = id,
                JournalHeaderId = 1,
                VoucherNo = "JV/2026-27/0001",
                RuleTypeId = 131,
                RuleTypeName = "Amount Over",
                Value = 2500000m,
                FlaggedAt = DateTimeOffset.UtcNow,
                DigestSent = false
            };

        public static FinanceManagement.Domain.Entities.JournalThresholdRule ValidEntity(int id = 1) =>
            new()
            {
                Id = id,
                RuleTypeId = 131,
                ThresholdValue = 5000000m,
                Active = true,
                EffectiveFrom = new DateOnly(2026, 4, 1),
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
