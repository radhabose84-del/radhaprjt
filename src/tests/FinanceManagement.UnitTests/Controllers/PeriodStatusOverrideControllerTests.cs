using Microsoft.AspNetCore.Mvc;
using FinanceManagement.Presentation.Controllers;
using FinanceManagement.Application.PeriodStatusOverride.Commands.ApprovePeriodReversal;
using FinanceManagement.Application.PeriodStatusOverride.Commands.RejectPeriodReversal;
using FinanceManagement.Application.PeriodStatusOverride.Commands.RequestPeriodReversal;
using FinanceManagement.Application.PeriodStatusOverride.Queries.GetPendingPeriodReversals;
using FinanceManagement.Application.PeriodStatusOverride.Dto;

namespace FinanceManagement.UnitTests.Controllers
{
    public sealed class PeriodStatusOverrideControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private PeriodStatusOverrideController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetPending_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPendingPeriodReversalsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<PeriodStatusOverrideDto>)new List<PeriodStatusOverrideDto>());

            var result = await CreateSut().GetPendingAsync();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Request_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<RequestPeriodReversalCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().RequestAsync(new RequestPeriodReversalCommand
            {
                PeriodId = 1, TargetStatusCode = "SOFTCLOSED", RequestedReason = "test"
            });
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Approve_PassesRouteIdIntoCommand()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<ApprovePeriodReversalCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var bodyCmd = new ApprovePeriodReversalCommand { Role = "CFO" };
            await CreateSut().ApproveAsync(overrideId: 42, command: bodyCmd);

            _mockMediator.Verify(m => m.Send(
                It.Is<ApprovePeriodReversalCommand>(c => c.OverrideId == 42 && c.Role == "CFO"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Reject_PassesRouteIdIntoCommand()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<RejectPeriodReversalCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var bodyCmd = new RejectPeriodReversalCommand { RejectionReason = "x" };
            await CreateSut().RejectAsync(overrideId: 42, command: bodyCmd);

            _mockMediator.Verify(m => m.Send(
                It.Is<RejectPeriodReversalCommand>(c => c.OverrideId == 42),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
