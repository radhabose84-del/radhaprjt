using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserGroup;
using UserManagement.Application.UserGroup.Commands.UpdateUesrGroup;
using UserManagement.Application.UserGroup.Queries.GetUserGroup;
using UserManagement.Domain.Enums.Common;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.UserGroup.Commands
{
    public sealed class UpdateUserGroupCommandHandlerTests
    {
        private readonly Mock<IUserGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IUserGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateUserGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockQueryRepo.Object, _mockMediator.Object);

        private static UserManagement.Domain.Entities.UserGroup BuildActiveEntity(int id = 1) =>
            new()
            {
                Id = id,
                GroupCode = "GRP001",
                GroupName = "Original Group",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            // Arrange
            var command = new UpdateUserGroupCommand { Id = 999, GroupName = "Updated", IsActive = 1 };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((UserManagement.Domain.Entities.UserGroup)null!);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_ValidUpdate_ReturnsTrue()
        {
            // Arrange
            var command = new UpdateUserGroupCommand { Id = 1, GroupName = "Updated Group", IsActive = 1 };
            var existing = BuildActiveEntity();
            var updatedEntity = new UserManagement.Domain.Entities.UserGroup { Id = 1, GroupCode = "GRP001", GroupName = "Updated Group" };
            var updatedDto = new UserGroupDto { Id = 1, GroupCode = "GRP001", GroupName = "Updated Group" };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existing);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(1, existing))
                .ReturnsAsync(1);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.UserGroup>(command))
                .Returns(updatedEntity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(1, updatedEntity))
                .ReturnsAsync(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existing);

            _mockMapper
                .Setup(m => m.Map<UserGroupDto>(existing))
                .Returns(updatedDto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_IsActiveChange_ReturnsTrueWithoutFullUpdate()
        {
            // Arrange
            var command = new UpdateUserGroupCommand { Id = 1, GroupName = "Test Group", IsActive = 0 };
            var existing = BuildActiveEntity();
            // IsActive on entity is Active (1), command sends 0 — triggers the early-return branch
            existing.IsActive = Enums.Status.Active; // byte value 1, command sends 0 → branch fires

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existing);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(1, existing))
                .ReturnsAsync(1);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidUpdate_PublishesAuditEvent()
        {
            // Arrange
            var command = new UpdateUserGroupCommand { Id = 1, GroupName = "Updated Group", IsActive = 1 };
            var existing = BuildActiveEntity();
            var updatedEntity = new UserManagement.Domain.Entities.UserGroup { Id = 1, GroupCode = "GRP001", GroupName = "Updated Group" };
            var updatedDto = new UserGroupDto { Id = 1, GroupCode = "GRP001", GroupName = "Updated Group" };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existing);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(1, existing))
                .ReturnsAsync(1);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.UserGroup>(command))
                .Returns(updatedEntity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(1, updatedEntity))
                .ReturnsAsync(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existing);

            _mockMapper
                .Setup(m => m.Map<UserGroupDto>(existing))
                .Returns(updatedDto);

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
                        e.ActionDetail == "Update" &&
                        e.Module == "UserGroup"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
