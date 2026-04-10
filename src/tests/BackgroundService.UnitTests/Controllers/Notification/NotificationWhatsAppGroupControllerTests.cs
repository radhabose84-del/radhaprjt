using MediatR;
using Microsoft.AspNetCore.Mvc;
using BackgroundService.Presentation.Controllers.Notification;
using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Commands.CreateNotificationWhatsAppGroup;
using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Commands.UpdateNotificationWhatsAppGroup;
using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Commands.DeleteNotificationWhatsAppGroup;
using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Queries.GetAllNotificationWhatsAppGroup;
using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Queries.GetNotificationWhatsAppGroupById;
using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Queries.GetNotificationWhatsAppGroupAutoComplete;
using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Queries.GetNotificationWhatsAppGroupByDepartment;
using BackgroundService.Application.Dto;

namespace BackgroundService.UnitTests.Controllers.Notification
{
    public sealed class NotificationWhatsAppGroupControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private NotificationWhatsAppGroupController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAllAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllNotificationWhatsAppGroupQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((
                    new List<NotificationWhatsAppGroupDto>(),
                    0,
                    1,
                    10
                ));

            var result = await CreateSut().GetAllAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllAsync_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllNotificationWhatsAppGroupQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((
                    new List<NotificationWhatsAppGroupDto>(),
                    0,
                    1,
                    10
                ));

            await CreateSut().GetAllAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllNotificationWhatsAppGroupQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateNotificationWhatsAppGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(new CreateNotificationWhatsAppGroupCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateNotificationWhatsAppGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateAsync(new UpdateNotificationWhatsAppGroupCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteNotificationWhatsAppGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteAsync_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteNotificationWhatsAppGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteAsync(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteNotificationWhatsAppGroupCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetNotificationWhatsAppGroupByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new NotificationWhatsAppGroupDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoCompleteAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetNotificationWhatsAppGroupAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<NotificationWhatsAppGroupAutoCompleteDto>());

            var result = await CreateSut().AutoCompleteAsync("test");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByDepartmentAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetNotificationWhatsAppGroupByDepartmentQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<NotificationWhatsAppGroupAutoCompleteDto>());

            var result = await CreateSut().GetByDepartmentAsync(1, "test");

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
