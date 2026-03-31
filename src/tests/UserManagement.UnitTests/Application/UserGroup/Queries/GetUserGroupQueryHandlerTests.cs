using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserGroup;
using UserManagement.Application.UserGroup.Queries.GetUserGroup;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.UserGroup.Queries
{
    public sealed class GetUserGroupQueryHandlerTests
    {
        private readonly Mock<IUserGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetUserGroupQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.UserGroup>
            {
                new() { Id = 1, GroupCode = "GRP001", GroupName = "Test Group" }
            };
            var dtos = new List<UserGroupDto>
            {
                new() { Id = 1, GroupCode = "GRP001", GroupName = "Test Group" }
            };

            _mockQueryRepo
                .Setup(r => r.GetAllUserGroupAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<UserGroupDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetUserGroupQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsPaginationMetadata()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.UserGroup>
            {
                new() { Id = 1, GroupCode = "GRP001", GroupName = "Test Group" }
            };
            var dtos = new List<UserGroupDto> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetAllUserGroupAsync(2, 5, "Test"))
                .ReturnsAsync((entities, 15));

            _mockMapper
                .Setup(m => m.Map<List<UserGroupDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetUserGroupQuery { PageNumber = 2, PageSize = 5, SearchTerm = "Test" },
                CancellationToken.None);

            // Assert
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(15);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.UserGroup>();
            var dtos = new List<UserGroupDto>();

            _mockQueryRepo
                .Setup(r => r.GetAllUserGroupAsync(1, 10, null))
                .ReturnsAsync((entities, 0));

            _mockMapper
                .Setup(m => m.Map<List<UserGroupDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetUserGroupQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WithResults_PublishesAuditEvent()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.UserGroup>
            {
                new() { Id = 1, GroupCode = "GRP001", GroupName = "Test Group" }
            };
            var dtos = new List<UserGroupDto> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetAllUserGroupAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<UserGroupDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(
                new GetUserGroupQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetAll" &&
                        e.Module == "UserGroup"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
