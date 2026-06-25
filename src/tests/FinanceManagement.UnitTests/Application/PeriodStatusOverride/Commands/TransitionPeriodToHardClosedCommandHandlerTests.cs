using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IPeriodStatusOverride;
using FinanceManagement.Application.PeriodStatusOverride.Commands.TransitionPeriodToHardClosed;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.PeriodStatusOverride.Commands
{
    public sealed class TransitionPeriodToHardClosedCommandHandlerTests
    {
        private readonly Mock<IPeriodStatusOverrideCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IPeriodStatusOverrideQueryRepository>   _mockQueryRepo   = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService>  _mockTz = new(MockBehavior.Loose);
        private readonly Mock<IMediator>         _mockMediator = new(MockBehavior.Loose);

        private TransitionPeriodToHardClosedCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockIp.Object, _mockTz.Object, _mockMediator.Object);

        private void SetupSession()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
            _mockIp.Setup(x => x.GetUserId()).Returns(1);
            _mockTz.Setup(x => x.GetCurrentTime(It.IsAny<string?>())).Returns(DateTimeOffset.UtcNow);
        }

        [Fact]
        public async Task Handle_SoftClosedPeriod_TransitionsToHardClosed()
        {
            SetupSession();
            _mockQueryRepo.Setup(r => r.GetPeriodSnapshotAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(PeriodStatusOverrideBuilders.SoftClosedPeriodSnapshot());
            _mockQueryRepo.Setup(r => r.GetMiscMasterIdByCodeAsync("FPS", "HARDCLOSED")).ReturnsAsync(300);
            _mockCommandRepo.Setup(r => r.ApplyPeriodStatusChangeAsync(
                    1, 300, It.IsAny<int>(), It.IsAny<DateTimeOffset>(), null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(new TransitionPeriodToHardClosedCommand(1), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_OpenPeriod_Throws_MustGoViaSoftClose()
        {
            SetupSession();
            _mockQueryRepo.Setup(r => r.GetPeriodSnapshotAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(PeriodStatusOverrideBuilders.OpenPeriodSnapshot());

            Func<Task> act = () => CreateSut().Handle(new TransitionPeriodToHardClosedCommand(1), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*Illegal*");
        }

        [Fact]
        public async Task Handle_AlreadyHardClosed_Throws()
        {
            SetupSession();
            _mockQueryRepo.Setup(r => r.GetPeriodSnapshotAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(PeriodStatusOverrideBuilders.HardClosedPeriodSnapshot());

            Func<Task> act = () => CreateSut().Handle(new TransitionPeriodToHardClosedCommand(1), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*Illegal*");
        }

        [Fact]
        public async Task Handle_Success_PublishesPeriodStatusChangedEvent()
        {
            SetupSession();
            _mockQueryRepo.Setup(r => r.GetPeriodSnapshotAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(PeriodStatusOverrideBuilders.SoftClosedPeriodSnapshot());
            _mockQueryRepo.Setup(r => r.GetMiscMasterIdByCodeAsync("FPS", "HARDCLOSED")).ReturnsAsync(300);
            _mockCommandRepo.Setup(r => r.ApplyPeriodStatusChangeAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTimeOffset>(),
                    null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(new TransitionPeriodToHardClosedCommand(1), CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(
                It.Is<PeriodStatusChangedDomainEvent>(e =>
                    e.ToStatusCode == "HARDCLOSED" &&
                    !e.IsReversal),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
