using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserGroup;
using UserManagement.Application.UserGroup.Commands.DeleteUserGroup;
using UserManagement.Application.UserGroup.Queries.GetUserGroup;
using UserManagement.Domain.Enums.Common;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.UserGroup.Commands
{
    public sealed class DeleteUserGroupCommandHandlerTests
    {
        private readonly Mock<IUserGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IUserGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteUserGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockQueryRepo.Object, _mockMediator.Object);

        private static UserManagement.Domain.Entities.UserGroup BuildActiveEntity(int id = 1) =>
            new()
            {
                Id = id,
                GroupCode = "GRP001",
                GroupName = "Test Group",
                IsDeleted = Enums.IsDelete.NotDeleted,
                IsActive = Enums.Status.Active
            };

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            // Arrange
            var command = new DeleteUserGroupCommand { Id = 1 };
            var entity = BuildActiveEntity();
            var deletedEntity = new UserManagement.Domain.Entities.UserGroup { Id = 1, GroupCode = "GRP001", GroupName = "Test Group" };
            var dto = new UserGroupDto { Id = 1, GroupCode = "GRP001", GroupName = "Test Group" };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.UserGroup>(command))
                .Returns(deletedEntity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, deletedEntity))
                .ReturnsAsync(1);

            _mockMapper
                .Setup(m => m.Map<UserGroupDto>(deletedEntity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            // Arrange
            var command = new DeleteUserGroupCommand { Id = 999 };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((UserManagement.Domain.Entities.UserGroup)null!);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*does not exist*");
        }

        [Fact]
        public async Task Handle_AlreadyDeleted_ThrowsValidationException()
        {
            // Arrange
            var command = new DeleteUserGroupCommand { Id = 2 };
            var deletedEntity = new UserManagement.Domain.Entities.UserGroup
            {
                Id = 2,
                GroupCode = "GRP002",
                IsDeleted = Enums.IsDelete.Deleted
            };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(2))
                .ReturnsAsync(deletedEntity);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Handle_DeleteFails_ThrowsException()
        {
            // Arrange
            var command = new DeleteUserGroupCommand { Id = 1 };
            var entity = BuildActiveEntity();
            var deletedEntity = new UserManagement.Domain.Entities.UserGroup { Id = 1 };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.UserGroup>(command))
                .Returns(deletedEntity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, deletedEntity))
                .ReturnsAsync(0);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*deletion failed*");
        }
    }
}
