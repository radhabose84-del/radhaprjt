using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICoaChangeRequest;
using FinanceManagement.Application.Common.Options;
using FinanceManagement.Application.CoaChangeRequest.Commands.ApproveCoaChangeImpact;
using FinanceManagement.Domain.Entities;
using Microsoft.Extensions.Options;

namespace FinanceManagement.UnitTests.Application.CoaChangeRequest.Commands
{
    public sealed class ApproveCoaChangeImpactCommandHandlerTests
    {
        private const int CfoRoleId = 15;

        private readonly Mock<ICoaChangeRequestCommandRepository> _mockCmd = new(MockBehavior.Loose);
        private readonly Mock<IRoleUserLookup> _mockRoles = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private ApproveCoaChangeImpactCommandHandler CreateSut()
        {
            var options = Options.Create(new CoaUnfreezeOptions { CfoRoleId = CfoRoleId });
            return new ApproveCoaChangeImpactCommandHandler(
                _mockCmd.Object, _mockRoles.Object, _mockIp.Object, _mockTz.Object, options, _mockMediator.Object);
        }

        // AC5 — only the CFO may approve the impact assessment.
        [Fact]
        public async Task Handle_NonCfo_IsBlocked()
        {
            _mockIp.Setup(x => x.GetUserId()).Returns(5);
            _mockTz.Setup(x => x.GetCurrentTime(It.IsAny<string>())).Returns(DateTimeOffset.UnixEpoch);
            _mockRoles.Setup(r => r.UserHasRoleAsync(5, CfoRoleId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var act = async () => await CreateSut().Handle(new ApproveCoaChangeImpactCommand { ChangeRequestId = 1 }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*Only the CFO*");
        }

        [Fact]
        public async Task Handle_Cfo_ApprovesImpactAndSetsStatus()
        {
            _mockIp.Setup(x => x.GetUserId()).Returns(15);
            _mockTz.Setup(x => x.GetCurrentTime(It.IsAny<string>())).Returns(new DateTimeOffset(2026, 6, 22, 10, 0, 0, TimeSpan.Zero));
            _mockRoles.Setup(r => r.UserHasRoleAsync(15, CfoRoleId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var entity = new FinanceManagement.Domain.Entities.CoaChangeRequest
            {
                Id = 1,
                RequestStatus = CoaChangeRequestStatus.PendingImpactApproval
            };
            _mockCmd.Setup(r => r.GetChangeRequestAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

            var result = await CreateSut().Handle(new ApproveCoaChangeImpactCommand { ChangeRequestId = 1 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            entity.RequestStatus.Should().Be(CoaChangeRequestStatus.ImpactApproved);
            entity.ImpactApprovedByUserId.Should().Be(15);
            entity.ImpactApprovedOn.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NotPendingImpactApproval_IsBlocked()
        {
            _mockIp.Setup(x => x.GetUserId()).Returns(15);
            _mockTz.Setup(x => x.GetCurrentTime(It.IsAny<string>())).Returns(DateTimeOffset.UnixEpoch);
            _mockRoles.Setup(r => r.UserHasRoleAsync(15, CfoRoleId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockCmd.Setup(r => r.GetChangeRequestAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FinanceManagement.Domain.Entities.CoaChangeRequest { Id = 1, RequestStatus = CoaChangeRequestStatus.ImpactApproved });

            var act = async () => await CreateSut().Handle(new ApproveCoaChangeImpactCommand { ChangeRequestId = 1 }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
