using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BackgroundService.Presentation.Controllers.Notification;
using BackgroundService.Application.Notification.NotificationGroupMember.Commands.CreateNotificationGroupMember;
using BackgroundService.Application.Notification.NotificationGroupMember.Commands.UpdateNotificationGroupMember;
using BackgroundService.Application.Notification.NotificationGroupMember.Queries.GetAllNotificationGroupMember;
using BackgroundService.Application.Notification.NotificationGroupMember.Queries.GetNotificationGroupById;

namespace BackgroundService.UnitTests.Controllers.Notification
{
    public sealed class NotificationGroupMemberControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private NotificationGroupMemberController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAllNotificationGroupMemberAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllNotificationGroupMembersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetNotificationGroupMemberDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetNotificationGroupMemberDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllNotificationGroupMemberAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllNotificationGroupMemberAsync_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllNotificationGroupMembersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetNotificationGroupMemberDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetNotificationGroupMemberDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllNotificationGroupMemberAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllNotificationGroupMembersQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateNotificationGroupMemberCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(new CreateNotificationGroupMemberCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateNotificationGroupMemberCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateAsync(new UpdateNotificationGroupMemberCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_WhenSuccess_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetNotificationGroupByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<GetNotificationGroupMemberDto>
                {
                    IsSuccess = true,
                    Data = new GetNotificationGroupMemberDto(),
                    Message = "Success"
                });

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ReturnsNotFoundResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetNotificationGroupByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<GetNotificationGroupMemberDto>
                {
                    IsSuccess = false,
                    Message = "Not found"
                });

            var result = await CreateSut().GetByIdAsync(999);

            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
