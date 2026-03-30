using BudgetManagement.Application.BudgetAllocation.Command.Create;
using BudgetManagement.Application.BudgetAllocation.Queries.GetRemainingBalance;
using BudgetManagement.Presentation.Controllers;
using BudgetManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BudgetManagement.UnitTests.Controllers
{
    public sealed class BudgetAllocationControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private BudgetAllocationController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateBudgetAllocationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(BudgetAllocationBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateBudgetAllocationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().CreateAsync(BudgetAllocationBuilders.ValidCreateCommand());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateBudgetAllocationCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetRemainingBalance_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetRemainingBalanceQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RemainingBalanceWithPrevDto
                {
                    BudgetGroupId = 1,
                    CurrentRemainingBalance = 40000m,
                    PreviousRemainingBalance = 50000m
                });

            var result = await CreateSut().GetRemainingBalance(
                budgetGroupId: 1,
                date: null,
                monthId: null,
                requestById: null,
                ProjectId: null,
                WbsId: null,
                financialYearId: 1,
                CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
