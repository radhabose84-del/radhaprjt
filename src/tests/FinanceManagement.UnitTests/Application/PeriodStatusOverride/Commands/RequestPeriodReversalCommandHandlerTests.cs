using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IPeriodStatusOverride;
using FinanceManagement.Application.PeriodStatusOverride.Commands.RequestPeriodReversal;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.PeriodStatusOverride.Commands
{
    public sealed class RequestPeriodReversalCommandHandlerTests
    {
        private readonly Mock<IPeriodStatusOverrideCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IPeriodStatusOverrideQueryRepository>   _mockQueryRepo   = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService>  _mockTz = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper>   _mockMapper   = new(MockBehavior.Loose);

        private RequestPeriodReversalCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockIp.Object, _mockTz.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupSession(int userId = 42)
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
            _mockIp.Setup(x => x.GetUserId()).Returns(userId);
            _mockTz.Setup(x => x.GetCurrentTime(It.IsAny<string?>())).Returns(DateTimeOffset.UtcNow);
        }

        private RequestPeriodReversalCommand ValidCommand(string targetCode = "SOFTCLOSED") =>
            new() { PeriodId = 1, TargetStatusCode = targetCode, RequestedReason = "Audit correction" };

        [Fact]
        public async Task Handle_HardClosedToSoftClosed_CreatesPendingOverride()
        {
            SetupSession();
            _mockQueryRepo.Setup(r => r.GetPeriodSnapshotAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(PeriodStatusOverrideBuilders.HardClosedPeriodSnapshot());
            _mockQueryRepo.Setup(r => r.HasOpenOverrideAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.GetMiscMasterIdByCodeAsync("FPS", "SOFTCLOSED")).ReturnsAsync(200);
            _mockQueryRepo.Setup(r => r.GetMiscMasterIdByCodeAsync("PSO", "PENDING")).ReturnsAsync(400);

            _mockMapper.Setup(m => m.Map<FinanceManagement.Domain.Entities.PeriodStatusOverride>(It.IsAny<RequestPeriodReversalCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.PeriodStatusOverride());
            _mockCommandRepo.Setup(r => r.CreateAsync(
                It.IsAny<FinanceManagement.Domain.Entities.PeriodStatusOverride>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(99);

            var result = await CreateSut().Handle(ValidCommand("SOFTCLOSED"), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(99);
        }

        [Fact]
        public async Task Handle_InvalidReversal_OpenToHardClosed_Throws()
        {
            SetupSession();
            _mockQueryRepo.Setup(r => r.GetPeriodSnapshotAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(PeriodStatusOverrideBuilders.OpenPeriodSnapshot());

            Func<Task> act = () => CreateSut().Handle(ValidCommand("HARDCLOSED"), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*not allowed*");
        }

        [Fact]
        public async Task Handle_HasOpenOverride_Throws()
        {
            SetupSession();
            _mockQueryRepo.Setup(r => r.GetPeriodSnapshotAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(PeriodStatusOverrideBuilders.HardClosedPeriodSnapshot());
            _mockQueryRepo.Setup(r => r.HasOpenOverrideAsync(1)).ReturnsAsync(true);

            Func<Task> act = () => CreateSut().Handle(ValidCommand("SOFTCLOSED"), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*already in progress*");
        }

        [Fact]
        public async Task Handle_PeriodNotFound_Throws()
        {
            SetupSession();
            _mockQueryRepo.Setup(r => r.GetPeriodSnapshotAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PeriodSnapshotDto?)null);

            Func<Task> act = () => CreateSut().Handle(ValidCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_PsoOpenStatusNotSeeded_Throws()
        {
            SetupSession();
            _mockQueryRepo.Setup(r => r.GetPeriodSnapshotAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(PeriodStatusOverrideBuilders.HardClosedPeriodSnapshot());
            _mockQueryRepo.Setup(r => r.HasOpenOverrideAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.GetMiscMasterIdByCodeAsync("FPS", "SOFTCLOSED")).ReturnsAsync(200);
            _mockQueryRepo.Setup(r => r.GetMiscMasterIdByCodeAsync("PSO", "PENDING")).ReturnsAsync(0);

            Func<Task> act = () => CreateSut().Handle(ValidCommand("SOFTCLOSED"), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*not seeded*");
        }
    }
}
