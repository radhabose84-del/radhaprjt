using MediatR;
using Microsoft.AspNetCore.Mvc;
using BackgroundService.Presentation.Controllers.Notification;
using BackgroundService.Application.Notification.GetNotificationDetail.GetNotificationDetailById;
using BackgroundService.Application.Notification.GetNotificationDetail.UpdateNotificationStatus;

namespace BackgroundService.UnitTests.Controllers.Notification
{
    public sealed class NotificationDetailControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private NotificationDetailController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetNotificationDetailByIdAsync_WithData_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetNotificationDetailByUserId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetNotificationDetailDto> { new GetNotificationDetailDto() });

            var result = await CreateSut().GetNotificationDetailByIdAsync("user1");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetNotificationDetailByIdAsync_WithEmptyData_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetNotificationDetailByUserId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetNotificationDetailDto>());

            var result = await CreateSut().GetNotificationDetailByIdAsync("user1");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetNotificationDetailByIdAsync_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetNotificationDetailByUserId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetNotificationDetailDto>());

            await CreateSut().GetNotificationDetailByIdAsync("user1");

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetNotificationDetailByUserId>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateNotificationStatus>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().UpdateAsync(new UpdateNotificationStatus());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateNotificationStatus>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().UpdateAsync(new UpdateNotificationStatus());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<UpdateNotificationStatus>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
