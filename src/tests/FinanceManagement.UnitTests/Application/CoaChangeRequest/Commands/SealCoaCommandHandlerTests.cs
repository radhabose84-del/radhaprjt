using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICoaFreeze;
using FinanceManagement.Application.Common.Options;
using FinanceManagement.Application.CoaChangeRequest.Commands.SealCoa;
using Microsoft.Extensions.Options;

namespace FinanceManagement.UnitTests.Application.CoaChangeRequest.Commands
{
    public sealed class SealCoaCommandHandlerTests
    {
        private const int CfoRoleId = 15;

        private readonly Mock<ICoaFreezeCommandRepository> _mockFreeze = new(MockBehavior.Loose);
        private readonly Mock<IRoleUserLookup> _mockRoles = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private SealCoaCommandHandler CreateSut()
        {
            var options = Options.Create(new CoaUnfreezeOptions { CfoRoleId = CfoRoleId });
            return new SealCoaCommandHandler(
                _mockFreeze.Object, _mockRoles.Object, _mockIp.Object, _mockTz.Object, options, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_NonCfo_IsBlocked()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
            _mockIp.Setup(x => x.GetUserId()).Returns(5);
            _mockTz.Setup(x => x.GetCurrentTime(It.IsAny<string>())).Returns(DateTimeOffset.UnixEpoch);
            _mockRoles.Setup(r => r.UserHasRoleAsync(5, CfoRoleId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var act = async () => await CreateSut().Handle(new SealCoaCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*Only the CFO*");
            _mockFreeze.Verify(f => f.FreezeAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Cfo_SealsCoa()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
            _mockIp.Setup(x => x.GetUserId()).Returns(15);
            _mockTz.Setup(x => x.GetCurrentTime(It.IsAny<string>())).Returns(new DateTimeOffset(2026, 6, 22, 10, 0, 0, TimeSpan.Zero));
            _mockRoles.Setup(r => r.UserHasRoleAsync(15, CfoRoleId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateSut().Handle(new SealCoaCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            _mockFreeze.Verify(f => f.FreezeAsync(1, 15, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
