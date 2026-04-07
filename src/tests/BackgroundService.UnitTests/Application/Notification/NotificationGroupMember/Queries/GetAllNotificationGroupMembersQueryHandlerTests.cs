using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroupMembers;
using BackgroundService.Application.Notification.NotificationGroupMember.Queries.GetAllNotificationGroupMember;
using MediatR;

namespace BackgroundService.UnitTests.Application.Notification.NotificationGroupMember.Queries
{
    public sealed class GetAllNotificationGroupMembersQueryHandlerTests
    {
        private readonly Mock<INotificationGroupMemberQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetAllNotificationGroupMembersQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<GetNotificationGroupMemberDto>
            {
                new() { GroupId = 1, GroupName = "TestGroup", Users = new List<UserDto> { new() { UserId = 10, UserName = "User1" } } }
            };

            _mockQueryRepo
                .Setup(r => r.GetAllNotificationGroupAsync(1, 15, null))
                .ReturnsAsync((dtoList, 1));

            _mockMapper
                .Setup(m => m.Map<List<GetNotificationGroupMemberDto>>(It.IsAny<List<GetNotificationGroupMemberDto>>()))
                .Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetAllNotificationGroupMembersQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllNotificationGroupAsync(1, 15, null))
                .ReturnsAsync((new List<GetNotificationGroupMemberDto>(), 0));

            _mockMapper
                .Setup(m => m.Map<List<GetNotificationGroupMemberDto>>(It.IsAny<List<GetNotificationGroupMemberDto>>()))
                .Returns(new List<GetNotificationGroupMemberDto>());

            var result = await CreateSut().Handle(
                new GetAllNotificationGroupMembersQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
