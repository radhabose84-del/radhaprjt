using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IPeriodStatusOverride;
using FinanceManagement.Application.PeriodStatusOverride.Commands.RejectPeriodReversal;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.PeriodStatusOverride.Commands
{
    public sealed class RejectPeriodReversalCommandHandlerTests
    {
        private readonly Mock<IPeriodStatusOverrideCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IPeriodStatusOverrideQueryRepository>   _mockQueryRepo   = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService>  _mockTz = new(MockBehavior.Loose);
        private readonly Mock<IMediator>         _mockMediator = new(MockBehavior.Loose);

        private RejectPeriodReversalCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockIp.Object, _mockTz.Object, _mockMediator.Object);

        private void SetupSession(int userId = 99)
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
            _mockIp.Setup(x => x.GetUserId()).Returns(userId);
            _mockTz.Setup(x => x.GetCurrentTime(It.IsAny<string?>())).Returns(DateTimeOffset.UtcNow);
        }

        [Fact]
        public async Task Handle_PendingOverride_Rejects()
        {
            SetupSession();
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(
                PeriodStatusOverrideBuilders.PendingOverrideDto());
            _mockQueryRepo.Setup(r => r.GetMiscMasterIdByCodeAsync("PSO", "REJECTED")).ReturnsAsync(600);
            _mockCommandRepo.Setup(r => r.UpdateAsync(
                It.IsAny<FinanceManagement.Domain.Entities.PeriodStatusOverride>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var result = await CreateSut().Handle(
                new RejectPeriodReversalCommand { OverrideId = 1, RejectionReason = "Not approved" },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_RequesterCannotSelfReject()
        {
            SetupSession(userId: 42);    // same as RequestedBy in builder
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(
                PeriodStatusOverrideBuilders.PendingOverrideDto(requestedBy: 42));

            Func<Task> act = () => CreateSut().Handle(
                new RejectPeriodReversalCommand { OverrideId = 1, RejectionReason = "test" }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*self-reject*");
        }

        [Fact]
        public async Task Handle_AlreadyFinalised_Throws()
        {
            SetupSession();
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(
                PeriodStatusOverrideBuilders.PendingOverrideDto(statusCode: "APPLIED"));

            Func<Task> act = () => CreateSut().Handle(
                new RejectPeriodReversalCommand { OverrideId = 1, RejectionReason = "x" }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*finalised*");
        }
    }
}
