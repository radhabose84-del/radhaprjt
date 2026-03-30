using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserGroup;
using UserManagement.Application.UserGroup.Commands.CreateUserGroup;
using UserManagement.Application.UserGroup.Queries.GetUserGroup;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.UserGroup.Commands
{
    public sealed class CreateUserGroupCommandHandlerTests
    {
        private readonly Mock<IUserGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateUserGroupCommandHandler CreateSut() =>
            new(_mockMapper.Object, _mockCommandRepo.Object, _mockMediator.Object);

        private static UserManagement.Domain.Entities.UserGroup BuildEntity(int id = 1) =>
            new() { Id = id, GroupCode = "GRP001", GroupName = "Test Group" };

        private static UserGroupDto BuildDto(int id = 1) =>
            new() { Id = id, GroupCode = "GRP001", GroupName = "Test Group" };

        [Fact]
        public async Task Handle_NewGroup_ReturnsDto()
        {
            // Arrange
            var command = new CreateUserGroupCommand { GroupCode = "GRP001", GroupName = "Test Group" };
            var existingCheck = new UserManagement.Domain.Entities.UserGroup { Id = 0 };
            var entity = BuildEntity();
            var dto = BuildDto();

            _mockCommandRepo
                .Setup(r => r.GetUserGroupByCodeAsync(command.GroupName!, command.GroupCode!))
                .ReturnsAsync(existingCheck);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.UserGroup>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(entity))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<UserGroupDto>(entity))
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
            result.GroupCode.Should().Be("GRP001");
        }

        [Fact]
        public async Task Handle_DuplicateGroupCode_ThrowsValidationException()
        {
            // Arrange
            var command = new CreateUserGroupCommand { GroupCode = "GRP001", GroupName = "Test Group" };
            var existing = new UserManagement.Domain.Entities.UserGroup { Id = 5, GroupCode = "GRP001" };

            _mockCommandRepo
                .Setup(r => r.GetUserGroupByCodeAsync(command.GroupName!, command.GroupCode!))
                .ReturnsAsync(existing);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task Handle_CreateReturnsNull_ThrowsException()
        {
            // Arrange
            var command = new CreateUserGroupCommand { GroupCode = "GRP001", GroupName = "Test Group" };
            var existingCheck = new UserManagement.Domain.Entities.UserGroup { Id = 0 };
            var entity = BuildEntity();

            _mockCommandRepo
                .Setup(r => r.GetUserGroupByCodeAsync(command.GroupName!, command.GroupCode!))
                .ReturnsAsync(existingCheck);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.UserGroup>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(entity))
                .ReturnsAsync((UserManagement.Domain.Entities.UserGroup)null!);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*not created*");
        }

        [Fact]
        public async Task Handle_NewGroup_PublishesAuditEvent()
        {
            // Arrange
            var command = new CreateUserGroupCommand { GroupCode = "GRP001", GroupName = "Test Group" };
            var existingCheck = new UserManagement.Domain.Entities.UserGroup { Id = 0 };
            var entity = BuildEntity();
            var dto = BuildDto();

            _mockCommandRepo
                .Setup(r => r.GetUserGroupByCodeAsync(command.GroupName!, command.GroupCode!))
                .ReturnsAsync(existingCheck);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.UserGroup>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(entity))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<UserGroupDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.Module == "USerGroup"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
