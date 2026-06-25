using Microsoft.AspNetCore.Mvc;
using FinanceManagement.Presentation.Controllers;
using FinanceManagement.Application.PeriodStatusOverride.Commands.TransitionPeriodToSoftClosed;
using FinanceManagement.Application.PeriodStatusOverride.Commands.TransitionPeriodToHardClosed;
using FinanceManagement.Application.PeriodStatusOverride.Queries.GetFinancialPeriodStatus;
using FinanceManagement.Application.PeriodStatusOverride.Queries.GetPeriodStatusHistory;
using FinanceManagement.Application.PeriodStatusOverride.Dto;

namespace FinanceManagement.UnitTests.Controllers
{
    public sealed class FinancialPeriodStatusControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private FinancialPeriodStatusController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetStatus_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetFinancialPeriodStatusQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FinancialPeriodStatusDto());

            var result = await CreateSut().GetStatusAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetHistory_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPeriodStatusHistoryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<PeriodStatusOverrideDto>)new List<PeriodStatusOverrideDto>());

            var result = await CreateSut().GetHistoryAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task SoftClose_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<TransitionPeriodToSoftClosedCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().SoftCloseAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task HardClose_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<TransitionPeriodToHardClosedCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().HardCloseAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task SoftClose_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<TransitionPeriodToSoftClosedCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            await CreateSut().SoftCloseAsync(7);

            _mockMediator.Verify(m => m.Send(
                It.Is<TransitionPeriodToSoftClosedCommand>(c => c.PeriodId == 7),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
