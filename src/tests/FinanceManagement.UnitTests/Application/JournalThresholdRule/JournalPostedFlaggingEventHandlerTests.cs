using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalFlag;
using FinanceManagement.Application.JournalMaster.JournalThresholdRule.EventHandlers;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Domain.Events;
using Microsoft.Extensions.Logging;

namespace FinanceManagement.UnitTests.Application.JournalThresholdRule
{
    public sealed class JournalPostedFlaggingEventHandlerTests
    {
        private readonly Mock<IJournalFlagEngineRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);
        private readonly Mock<ILogger<JournalPostedFlaggingEventHandler>> _mockLogger = new(MockBehavior.Loose);

        private JournalPostedFlaggingEventHandler CreateSut()
        {
            _mockTz.Setup(t => t.GetCurrentTime(It.IsAny<string?>())).Returns(DateTimeOffset.UtcNow);
            return new(_mockRepo.Object, _mockTz.Object, _mockLogger.Object);
        }

        private void SetupRules(params ActiveThresholdRule[] rules) =>
            _mockRepo.Setup(r => r.GetActiveThresholdRulesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(rules.ToList());

        private List<JournalFlag>? CaptureAddFlags()
        {
            List<JournalFlag>? captured = null;
            _mockRepo.Setup(r => r.AddFlagsAsync(It.IsAny<IEnumerable<JournalFlag>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<JournalFlag>, CancellationToken>((f, _) => captured = f.ToList())
                .Returns(Task.CompletedTask);
            return captured;
        }

        private static JournalPostedDomainEvent Posted(decimal amount, DateOnly? date = null, bool reversal = false) =>
            new(journalId: 1, companyId: 1, amount: amount, postingDate: date ?? new DateOnly(2026, 6, 15), isReversal: reversal);

        [Fact]
        public async Task AmountOver_Breached_RaisesFlag()
        {
            SetupRules(new ActiveThresholdRule { RuleTypeId = 131, RuleTypeCode = "AMT_OVER", ThresholdValue = 5000000m });
            List<JournalFlag>? captured = null;
            _mockRepo.Setup(r => r.AddFlagsAsync(It.IsAny<IEnumerable<JournalFlag>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<JournalFlag>, CancellationToken>((f, _) => captured = f.ToList())
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(Posted(6000000m), CancellationToken.None);

            captured.Should().ContainSingle();
            captured![0].RuleTypeId.Should().Be(131);
            captured[0].JournalHeaderId.Should().Be(1);
            captured[0].Value.Should().Be(6000000m);
        }

        [Fact]
        public async Task AmountOver_NotBreached_NoFlag()
        {
            SetupRules(new ActiveThresholdRule { RuleTypeId = 131, RuleTypeCode = "AMT_OVER", ThresholdValue = 5000000m });

            // No AddFlagsAsync setup → Strict mock fails if it is (wrongly) called.
            await CreateSut().Handle(Posted(1000000m), CancellationToken.None);

            _mockRepo.Verify(r => r.AddFlagsAsync(It.IsAny<IEnumerable<JournalFlag>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RoundNumber_RaisesFlag()
        {
            SetupRules(new ActiveThresholdRule { RuleTypeId = 133, RuleTypeCode = "ROUND_NUM" });
            _ = CaptureAddFlags();

            await CreateSut().Handle(Posted(500000m), CancellationToken.None);

            _mockRepo.Verify(r => r.AddFlagsAsync(It.Is<IEnumerable<JournalFlag>>(f => f.Any()), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Weekend_RaisesFlag()
        {
            SetupRules(new ActiveThresholdRule { RuleTypeId = 136, RuleTypeCode = "WEEKEND_POST" });
            _ = CaptureAddFlags();

            var saturday = new DateOnly(2026, 6, 1);
            while (saturday.DayOfWeek != DayOfWeek.Saturday) saturday = saturday.AddDays(1);

            await CreateSut().Handle(Posted(1234m, date: saturday), CancellationToken.None);

            _mockRepo.Verify(r => r.AddFlagsAsync(It.Is<IEnumerable<JournalFlag>>(f => f.Any()), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Reversal_OnlyRaises_SameDayRev_NotAmountRules()
        {
            SetupRules(
                new ActiveThresholdRule { RuleTypeId = 131, RuleTypeCode = "AMT_OVER", ThresholdValue = 5000000m },
                new ActiveThresholdRule { RuleTypeId = 133, RuleTypeCode = "ROUND_NUM" },
                new ActiveThresholdRule { RuleTypeId = 137, RuleTypeCode = "SAME_DAY_REV" });

            List<JournalFlag>? captured = null;
            _mockRepo.Setup(r => r.AddFlagsAsync(It.IsAny<IEnumerable<JournalFlag>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<JournalFlag>, CancellationToken>((f, _) => captured = f.ToList())
                .Returns(Task.CompletedTask);

            // 6,000,000 is both over-threshold AND a round lakh, but it's a reversal → only SAME_DAY_REV fires.
            await CreateSut().Handle(Posted(6000000m, reversal: true), CancellationToken.None);

            captured.Should().ContainSingle();
            captured![0].RuleTypeId.Should().Be(137);
        }

        [Fact]
        public async Task NoActiveRules_NoFlag()
        {
            SetupRules();   // empty

            await CreateSut().Handle(Posted(9999999m), CancellationToken.None);

            _mockRepo.Verify(r => r.AddFlagsAsync(It.IsAny<IEnumerable<JournalFlag>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RepositoryThrows_IsSwallowed_NeverBlocksPosting()
        {
            _mockRepo.Setup(r => r.GetActiveThresholdRulesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("db down"));

            var act = async () => await CreateSut().Handle(Posted(6000000m), CancellationToken.None);

            await act.Should().NotThrowAsync();
        }
    }
}
