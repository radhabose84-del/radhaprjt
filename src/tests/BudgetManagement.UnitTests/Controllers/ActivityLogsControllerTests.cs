using BudgetManagement.Domain.Entities;
using BudgetManagement.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BudgetManagement.UnitTests.Controllers
{
    public sealed class ActivityLogsControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private ActivityLogsController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetActivityLogsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<ActivityLog>(), 0));

            var result = await CreateSut().GetAll("BudgetGroup", 1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetActivityLogsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<ActivityLog>(), 0));

            await CreateSut().GetAll("BudgetGroup", 1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetActivityLogsQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_WhenFound_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetActivityLogByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ActivityLog { Id = 1, EntityName = "BudgetGroup", EntityId = 1 });

            var result = await CreateSut().GetById(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_WhenNotFound_ReturnsNotFoundResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetActivityLogByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ActivityLog?)null);

            var result = await CreateSut().GetById(999);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetById_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetActivityLogByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ActivityLog { Id = 1, EntityName = "BudgetGroup", EntityId = 1 });

            await CreateSut().GetById(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetActivityLogByIdQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
