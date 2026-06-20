using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.CoaFreeze.Queries.GetCoaFreezeState;
using FinanceManagement.Application.Common.Interfaces.ICoaFreeze;

namespace FinanceManagement.UnitTests.Application.CoaFreeze
{
    public sealed class GetCoaFreezeStateQueryHandlerTests
    {
        private readonly Mock<ICoaFreezeQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<ICoaFreezeViolationLog> _mockLog = new(MockBehavior.Loose);
        private readonly Mock<IUserLookup> _mockUsers = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetCoaFreezeStateQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockLog.Object, _mockUsers.Object, _mockIp.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_Frozen_ReturnsBannerWithCountsRoleAndBlockedAttempts()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            var frozenOn = new DateTimeOffset(2026, 4, 14, 9, 41, 0, TimeSpan.FromHours(5.5));
            _mockRepo.Setup(r => r.GetStateAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CoaFreezeStateRow { IsFrozen = true, FrozenByUserId = 396, FrozenOn = frozenOn });
            _mockRepo.Setup(r => r.AreTriggersActiveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockRepo.Setup(r => r.GetCoaCountsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((24, 22));
            _mockLog.Setup(l => l.CountSinceAsync(1, frozenOn, It.IsAny<CancellationToken>())).ReturnsAsync(7);
            _mockUsers.Setup(u => u.GetByIdAsync(396, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserLookupDto { UserId = 396, FirstName = "Rajesh", LastName = "Gupta" });

            var dto = await CreateSut().Handle(new GetCoaFreezeStateQuery(), CancellationToken.None);

            dto.IsFrozen.Should().BeTrue();
            dto.DbTriggerActive.Should().BeTrue();
            dto.TotalAccounts.Should().Be(24);
            dto.TotalAccountGroups.Should().Be(22);
            dto.BlockedAttemptsSinceFreeze.Should().Be(7);
            dto.FrozenByName.Should().Be("Rajesh Gupta");
        }

        [Fact]
        public async Task Handle_NotFrozen_NoBlockedAttemptLookup()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockRepo.Setup(r => r.GetStateAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((CoaFreezeStateRow?)null);
            _mockRepo.Setup(r => r.AreTriggersActiveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockRepo.Setup(r => r.GetCoaCountsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((24, 22));

            var dto = await CreateSut().Handle(new GetCoaFreezeStateQuery(), CancellationToken.None);

            dto.IsFrozen.Should().BeFalse();
            dto.BlockedAttemptsSinceFreeze.Should().Be(0);
            dto.FrozenByName.Should().BeNull();
            _mockLog.Verify(l => l.CountSinceAsync(It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
