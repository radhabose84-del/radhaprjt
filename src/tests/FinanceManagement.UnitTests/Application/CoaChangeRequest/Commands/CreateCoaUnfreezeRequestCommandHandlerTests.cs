using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICoaChangeRequest;
using FinanceManagement.Application.Common.Options;
using FinanceManagement.Application.CoaChangeRequest.Commands.CreateCoaUnfreezeRequest;
using FinanceManagement.Domain.Entities;
using Microsoft.Extensions.Options;

namespace FinanceManagement.UnitTests.Application.CoaChangeRequest.Commands
{
    public sealed class CreateCoaUnfreezeRequestCommandHandlerTests
    {
        private readonly Mock<ICoaChangeRequestCommandRepository> _mockCmd = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateCoaUnfreezeRequestCommandHandler CreateSut()
        {
            var options = Options.Create(new CoaUnfreezeOptions { DefaultWindowMinutes = 60 });
            return new CreateCoaUnfreezeRequestCommandHandler(
                _mockCmd.Object, _mockIp.Object, _mockTz.Object, options, _mockMediator.Object);
        }

        private void SetupSession()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
            _mockIp.Setup(x => x.GetUserId()).Returns(3);
            _mockTz.Setup(x => x.GetCurrentTime(It.IsAny<string>())).Returns(new DateTimeOffset(2026, 6, 22, 10, 0, 0, TimeSpan.Zero));
        }

        // AC5 — a window cannot be raised without at least one CFO-impact-approved change request.
        [Fact]
        public async Task Handle_NoImpactApprovedRequests_IsBlocked()
        {
            SetupSession();
            _mockCmd.Setup(r => r.GetImpactApprovedChangeRequestsAsync(It.IsAny<IEnumerable<int>>(), 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FinanceManagement.Domain.Entities.CoaChangeRequest>());

            var act = async () => await CreateSut().Handle(
                new CreateCoaUnfreezeRequestCommand { Reason = "year-end fix", ChangeRequestIds = new List<int> { 1 } },
                CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*impact-approved*");
        }

        [Fact]
        public async Task Handle_ImpactApprovedRequests_CreatesWindowAndAttaches()
        {
            SetupSession();
            var crs = new List<FinanceManagement.Domain.Entities.CoaChangeRequest>
            {
                new() { Id = 1, RequestStatus = CoaChangeRequestStatus.ImpactApproved },
                new() { Id = 2, RequestStatus = CoaChangeRequestStatus.ImpactApproved }
            };
            _mockCmd.Setup(r => r.GetImpactApprovedChangeRequestsAsync(It.IsAny<IEnumerable<int>>(), 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(crs);
            _mockCmd.Setup(r => r.AddUnfreezeRequestWithoutSaveAsync(It.IsAny<CoaUnfreezeRequest>(), It.IsAny<CancellationToken>()))
                .Callback<CoaUnfreezeRequest, CancellationToken>((u, _) => u.Id = 50)
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new CreateCoaUnfreezeRequestCommand { Reason = "year-end fix", ChangeRequestIds = new List<int> { 1, 2 } },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(50);
            crs.Should().OnlyContain(c => c.UnfreezeRequestId == 50);
        }
    }
}
