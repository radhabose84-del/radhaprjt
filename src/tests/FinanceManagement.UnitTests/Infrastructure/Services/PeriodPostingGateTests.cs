using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IPeriodStatusOverride;
using FinanceManagement.Infrastructure.Services;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Infrastructure.Services
{
    /// <summary>
    /// US-GL03-02 — security gate for journal-posting middleware. Critical: every JE post hits this.
    /// Verifies all 3 status states + Period 13 adjustment guard against role mocks via IIPAddressService.
    /// </summary>
    public sealed class PeriodPostingGateTests
    {
        private readonly Mock<IPeriodStatusOverrideQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private PeriodPostingGate CreateSut() =>
            new(_mockQueryRepo.Object, _mockIp.Object);

        // ─── Period not found ───────────────────────────────────────────────

        [Fact]
        public async Task CheckPostingAllowedAsync_PeriodNotFound_ReturnsError()
        {
            _mockQueryRepo.Setup(r => r.GetPeriodSnapshotAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PeriodSnapshotDto?)null);

            var result = await CreateSut().CheckPostingAllowedAsync(99, 1, CancellationToken.None);
            result.Should().Be("Period not found.");
        }

        [Fact]
        public async Task CheckPostingAllowedAsync_CrossCompany_ReturnsError()
        {
            _mockQueryRepo.Setup(r => r.GetPeriodSnapshotAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(PeriodStatusOverrideBuilders.OpenPeriodSnapshot(companyId: 99));

            var result = await CreateSut().CheckPostingAllowedAsync(1, 1, CancellationToken.None);
            result.Should().Be("Period not found for this company.");
        }

        // ─── AC#1 — HARDCLOSED blocks EVERYONE (even CFO) ───────────────────

        [Fact]
        public async Task CheckPostingAllowedAsync_HardClosed_BlocksEveryone()
        {
            _mockQueryRepo.Setup(r => r.GetPeriodSnapshotAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(PeriodStatusOverrideBuilders.HardClosedPeriodSnapshot());
            _mockIp.Setup(x => x.IsInAnyRole(It.IsAny<IEnumerable<string>>())).Returns(true);   // even CFO

            var result = await CreateSut().CheckPostingAllowedAsync(1, 1, CancellationToken.None);
            result.Should().Contain("hard-closed");
        }

        // ─── AC#2 — SOFTCLOSED + role-check ─────────────────────────────────

        [Fact]
        public async Task CheckPostingAllowedAsync_SoftClosed_FinanceManagerAllowed()
        {
            _mockQueryRepo.Setup(r => r.GetPeriodSnapshotAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(PeriodStatusOverrideBuilders.SoftClosedPeriodSnapshot());
            _mockIp.Setup(x => x.IsInAnyRole(It.IsAny<IEnumerable<string>>())).Returns(true);

            var result = await CreateSut().CheckPostingAllowedAsync(1, 1, CancellationToken.None);
            result.Should().BeNull();
        }

        [Fact]
        public async Task CheckPostingAllowedAsync_SoftClosed_NonManagerBlocked()
        {
            _mockQueryRepo.Setup(r => r.GetPeriodSnapshotAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(PeriodStatusOverrideBuilders.SoftClosedPeriodSnapshot());
            _mockIp.Setup(x => x.IsInAnyRole(It.IsAny<IEnumerable<string>>())).Returns(false);

            var result = await CreateSut().CheckPostingAllowedAsync(1, 1, CancellationToken.None);
            result.Should().Contain("soft-closed");
        }

        // ─── OPEN period — always allowed (unless adjustment) ───────────────

        [Fact]
        public async Task CheckPostingAllowedAsync_OpenRegularPeriod_AllowsAnyone()
        {
            _mockQueryRepo.Setup(r => r.GetPeriodSnapshotAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(PeriodStatusOverrideBuilders.OpenPeriodSnapshot());
            _mockIp.Setup(x => x.IsInAnyRole(It.IsAny<IEnumerable<string>>())).Returns(false);

            var result = await CreateSut().CheckPostingAllowedAsync(1, 1, CancellationToken.None);
            result.Should().BeNull();
        }

        // ─── AC#3 (carry from US-GL03-01) — Period 13 (adjustment) role-gate ─

        [Fact]
        public async Task CheckPostingAllowedAsync_AdjustmentPeriod_FinanceControllerAllowed()
        {
            _mockQueryRepo.Setup(r => r.GetPeriodSnapshotAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(PeriodStatusOverrideBuilders.AdjustmentPeriodSnapshot());
            // In real code there are two IsInAnyRole calls (soft-close roles + adjustment roles).
            // For OPEN+IsAdjustment, soft-close check is skipped, only adjustment check fires.
            _mockIp.Setup(x => x.IsInAnyRole(It.IsAny<IEnumerable<string>>())).Returns(true);

            var result = await CreateSut().CheckPostingAllowedAsync(1, 1, CancellationToken.None);
            result.Should().BeNull();
        }

        [Fact]
        public async Task CheckPostingAllowedAsync_AdjustmentPeriod_NonControllerBlocked()
        {
            _mockQueryRepo.Setup(r => r.GetPeriodSnapshotAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(PeriodStatusOverrideBuilders.AdjustmentPeriodSnapshot());
            _mockIp.Setup(x => x.IsInAnyRole(It.IsAny<IEnumerable<string>>())).Returns(false);

            var result = await CreateSut().CheckPostingAllowedAsync(1, 1, CancellationToken.None);
            result.Should().Contain("adjustment");
        }
    }
}
