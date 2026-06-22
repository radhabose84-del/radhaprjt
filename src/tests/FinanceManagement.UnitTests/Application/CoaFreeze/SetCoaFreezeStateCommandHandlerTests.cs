using Contracts.Interfaces;
using FinanceManagement.Application.CoaFreeze.Commands.SetCoaFreezeState;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICoaFreeze;

namespace FinanceManagement.UnitTests.Application.CoaFreeze
{
    public sealed class SetCoaFreezeStateCommandHandlerTests
    {
        private readonly Mock<ICoaFreezeCommandRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private readonly DateTimeOffset _now = new(2026, 4, 14, 9, 41, 0, TimeSpan.Zero);

        private SetCoaFreezeStateCommandHandler CreateSut()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockIp.Setup(s => s.GetUserId()).Returns(396);
            _mockTz.Setup(t => t.GetCurrentTime(It.IsAny<string?>())).Returns(_now);
            return new(_mockRepo.Object, _mockIp.Object, _mockTz.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_Freeze_CallsFreezeAsync()
        {
            var result = await CreateSut().Handle(new SetCoaFreezeStateCommand { IsFrozen = true }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            _mockRepo.Verify(r => r.FreezeAsync(1, 396, _now, It.IsAny<CancellationToken>()), Times.Once);
        }

        // US-GL02-08B (G1): the TEST/ADMIN unfreeze branch is now blocked — unfreeze must go through the
        // governed dual-approval workflow. The hook may only seal.
        [Fact]
        public async Task Handle_OpenUnfreezeWindow_IsBlocked()
        {
            var act = async () => await CreateSut().Handle(
                new SetCoaFreezeStateCommand { IsFrozen = false, UnfreezeWindowMinutes = 30 }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*Direct unfreeze is disabled*");
            _mockRepo.Verify(r => r.OpenUnfreezeWindowAsync(It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
