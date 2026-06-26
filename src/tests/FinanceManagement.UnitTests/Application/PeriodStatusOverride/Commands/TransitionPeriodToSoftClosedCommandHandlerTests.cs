using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IPeriodStatusOverride;
using FinanceManagement.Application.PeriodStatusOverride.Commands.TransitionPeriodToSoftClosed;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.PeriodStatusOverride.Commands
{
    public sealed class TransitionPeriodToSoftClosedCommandHandlerTests
    {
        private readonly Mock<IPeriodStatusOverrideCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IPeriodStatusOverrideQueryRepository>   _mockQueryRepo   = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService>  _mockTz = new(MockBehavior.Loose);
        private readonly Mock<IMediator>         _mockMediator = new(MockBehavior.Loose);

        private TransitionPeriodToSoftClosedCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockIp.Object, _mockTz.Object, _mockMediator.Object);

        private void SetupSession(int companyId = 1, int userId = 1)
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(companyId);
            _mockIp.Setup(x => x.GetUserId()).Returns(userId);
            _mockTz.Setup(x => x.GetCurrentTime(It.IsAny<string?>())).Returns(DateTimeOffset.UtcNow);
        }

        [Fact]
        public async Task Handle_OpenPeriod_TransitionsToSoftClosed()
        {
            SetupSession();
            _mockQueryRepo.Setup(r => r.GetPeriodSnapshotAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(PeriodStatusOverrideBuilders.OpenPeriodSnapshot());
            _mockQueryRepo.Setup(r => r.GetMiscMasterIdByCodeAsync("FPS", "SOFTCLOSED")).ReturnsAsync(200);
            _mockCommandRepo.Setup(r => r.ApplyPeriodStatusChangeAsync(
                    1, 200, It.IsAny<int>(), It.IsAny<DateTimeOffset>(), null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(new TransitionPeriodToSoftClosedCommand(1), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NotOpen_Throws()
        {
            SetupSession();
            _mockQueryRepo.Setup(r => r.GetPeriodSnapshotAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(PeriodStatusOverrideBuilders.SoftClosedPeriodSnapshot());

            Func<Task> act = () => CreateSut().Handle(new TransitionPeriodToSoftClosedCommand(1), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*Illegal*");
        }

        [Fact]
        public async Task Handle_PeriodNotFound_Throws()
        {
            SetupSession();
            _mockQueryRepo.Setup(r => r.GetPeriodSnapshotAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PeriodSnapshotDto?)null);

            Func<Task> act = () => CreateSut().Handle(new TransitionPeriodToSoftClosedCommand(99), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_PeriodInDifferentCompany_Throws()
        {
            SetupSession(companyId: 1);
            _mockQueryRepo.Setup(r => r.GetPeriodSnapshotAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(PeriodStatusOverrideBuilders.OpenPeriodSnapshot(companyId: 99));

            Func<Task> act = () => CreateSut().Handle(new TransitionPeriodToSoftClosedCommand(1), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*not found for this company*");
        }

        [Fact]
        public async Task Handle_StatusNotSeeded_Throws()
        {
            SetupSession();
            _mockQueryRepo.Setup(r => r.GetPeriodSnapshotAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(PeriodStatusOverrideBuilders.OpenPeriodSnapshot());
            _mockQueryRepo.Setup(r => r.GetMiscMasterIdByCodeAsync("FPS", "SOFTCLOSED")).ReturnsAsync(0);

            Func<Task> act = () => CreateSut().Handle(new TransitionPeriodToSoftClosedCommand(1), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*not seeded*");
        }

        [Fact]
        public async Task Handle_NoCompany_Throws()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns((int?)null);

            Func<Task> act = () => CreateSut().Handle(new TransitionPeriodToSoftClosedCommand(1), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }

        [Fact]
        public async Task Handle_Success_PublishesPeriodStatusChangedEvent()
        {
            SetupSession();
            _mockQueryRepo.Setup(r => r.GetPeriodSnapshotAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(PeriodStatusOverrideBuilders.OpenPeriodSnapshot());
            _mockQueryRepo.Setup(r => r.GetMiscMasterIdByCodeAsync("FPS", "SOFTCLOSED")).ReturnsAsync(200);
            _mockCommandRepo.Setup(r => r.ApplyPeriodStatusChangeAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTimeOffset>(),
                    null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(new TransitionPeriodToSoftClosedCommand(1), CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(
                It.Is<PeriodStatusChangedDomainEvent>(e =>
                    e.ToStatusCode == "SOFTCLOSED" &&
                    !e.IsReversal),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
