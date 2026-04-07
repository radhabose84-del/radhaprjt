using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroupMembers;
using BackgroundService.Application.Notification.NotificationGroupMember.Queries.GetAllNotificationGroupMember;
using BackgroundService.Application.Notification.NotificationGroupMember.Queries.GetNotificationGroupById;
using BackgroundService.Application.Notification.NotificationGroupMember.Queries.GetNotificationGroupMemberById;
using MediatR;

namespace BackgroundService.UnitTests.Application.Notification.NotificationGroupMember.Queries
{
    public sealed class GetNotificationGroupByIdQueryHandlerTests
    {
        private readonly Mock<INotificationGroupMemberQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetNotificationGroupByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsSuccessWithDto()
        {
            var dto = new GetNotificationGroupMemberDto { GroupId = 1, GroupName = "TestGroup" };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);

            _mockMapper
                .Setup(m => m.Map<GetNotificationGroupMemberDto>(It.IsAny<GetNotificationGroupMemberDto>()))
                .Returns(dto);

            var result = await CreateSut().Handle(
                new GetNotificationGroupByIdQuery { Id = 1 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.GroupName.Should().Be("TestGroup");
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsIsSuccessFalse()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((GetNotificationGroupMemberDto?)null);

            var result = await CreateSut().Handle(
                new GetNotificationGroupByIdQuery { Id = 99 },
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Data.Should().BeNull();
        }
    }
}
