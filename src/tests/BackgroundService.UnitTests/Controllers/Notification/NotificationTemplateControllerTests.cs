using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BackgroundService.Presentation.Controllers.Notification;
using BackgroundService.Application.Notification.NotificationTemplate.Command.CreateNotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Command.DeleteNotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Command.UpdateNotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Queries.GetAllNotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Queries.GetNotificationTemplateAutoComplete;
using BackgroundService.Application.Notification.NotificationTemplate.Queries.GetNotificationTemplateById;

namespace BackgroundService.UnitTests.Controllers.Notification
{
    public sealed class NotificationTemplateControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private NotificationTemplateController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAllNotificationTemplateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllNotificationTemplateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<NotificationTemplateDto>>
                {
                    IsSuccess = true,
                    Data = new List<NotificationTemplateDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllNotificationTemplateAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllNotificationTemplateAsync_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllNotificationTemplateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<NotificationTemplateDto>>
                {
                    IsSuccess = true,
                    Data = new List<NotificationTemplateDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllNotificationTemplateAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllNotificationTemplateQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetNotificationTemplateAutoCompleteAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetNotificationTemplateAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<NotificationTemplateAutoCompleteDto>());

            var result = await CreateSut().GetNotificationTemplateAutoCompleteAsync("TestModule");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetNotificationTemplateByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new NotificationTemplateDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateNotificationTemplateCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(new CreateNotificationTemplateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateNotificationTemplateCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().UpdateAsync(new UpdateNotificationTemplateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteNotificationTemplateCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().DeleteAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteAsync_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteNotificationTemplateCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().DeleteAsync(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteNotificationTemplateCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
