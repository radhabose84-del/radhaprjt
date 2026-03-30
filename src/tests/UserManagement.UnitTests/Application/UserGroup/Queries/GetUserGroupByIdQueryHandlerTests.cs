using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserGroup;
using UserManagement.Application.UserGroup.Queries.GetUserGroup;
using UserManagement.Application.UserGroup.Queries.GetUserGroupById;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.UserGroup.Queries
{
    public sealed class GetUserGroupByIdQueryHandlerTests
    {
        private readonly Mock<IUserGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetUserGroupByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            // Arrange
            var entity = new UserManagement.Domain.Entities.UserGroup { Id = 1, GroupCode = "GRP001", GroupName = "Test Group" };
            var dto = new UserGroupDto { Id = 1, GroupCode = "GRP001", GroupName = "Test Group" };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<UserGroupDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(new GetUserGroupByIdQuery { Id = 1 }, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.GroupCode.Should().Be("GRP001");
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            // Arrange
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((UserManagement.Domain.Entities.UserGroup)null!);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () =>
                await sut.Handle(new GetUserGroupByIdQuery { Id = 999 }, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            // Arrange
            var entity = new UserManagement.Domain.Entities.UserGroup { Id = 1, GroupCode = "GRP001", GroupName = "Test Group" };
            var dto = new UserGroupDto { Id = 1, GroupCode = "GRP001", GroupName = "Test Group" };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<UserGroupDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(new GetUserGroupByIdQuery { Id = 1 }, CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetById" &&
                        e.Module == "UserGroup"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingId_CallsRepositoryOnce()
        {
            // Arrange
            var entity = new UserManagement.Domain.Entities.UserGroup { Id = 1, GroupCode = "GRP001", GroupName = "Test Group" };
            var dto = new UserGroupDto { Id = 1, GroupCode = "GRP001" };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<UserGroupDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(new GetUserGroupByIdQuery { Id = 1 }, CancellationToken.None);

            // Assert
            _mockQueryRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
        }
    }
}
