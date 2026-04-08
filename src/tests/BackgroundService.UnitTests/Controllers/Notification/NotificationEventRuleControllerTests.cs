using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BackgroundService.Presentation.Controllers.Notification;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.UpdateNotificationEventRule;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.InsertNotificationEventRule;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Queries.DeleteNotificationEventRule;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Queries.GetAllNotificationHierarchy;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Queries.GetNotificationHierarchyById;

namespace BackgroundService.UnitTests.Controllers.Notification
{
    public sealed class NotificationEventRuleControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private NotificationEventRuleController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllNotificationHierarchyQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<NotificationHierarchyAndEventRuleDto>>
                {
                    IsSuccess = true,
                    Data = new List<NotificationHierarchyAndEventRuleDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAll(new GetAllNotificationHierarchyQuery());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllNotificationHierarchyQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<NotificationHierarchyAndEventRuleDto>>
                {
                    IsSuccess = true,
                    Data = new List<NotificationHierarchyAndEventRuleDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAll(new GetAllNotificationHierarchyQuery());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllNotificationHierarchyQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_WhenFound_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetNotificationHierarchyByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new NotificationHierarchyAndEventRuleDto());

            var result = await CreateSut().GetById(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_WhenNotFound_ReturnsNotFoundResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetNotificationHierarchyByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((NotificationHierarchyAndEventRuleDto?)null);

            var result = await CreateSut().GetById(999);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Insert_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<InsertNotificationHierarchyAndEventRuleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Insert(new NotificationHierarchyAndEventRuleDto());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateNotificationHierarchyAndEventRuleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Update(new UpdateNotificationHierarchyAndEventRuleCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteNotificationLevelHierarchyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteNotificationLevelHierarchyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Delete(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteNotificationLevelHierarchyCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
