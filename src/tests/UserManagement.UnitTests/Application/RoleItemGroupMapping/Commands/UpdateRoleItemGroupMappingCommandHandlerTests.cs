using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Commands.UpdateRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Dto;
using UserManagement.Domain.Enums.Common;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.RoleItemGroupMapping.Commands
{
    public sealed class UpdateRoleItemGroupMappingCommandHandlerTests
    {
        private readonly Mock<IRoleItemGroupMappingCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IRoleItemGroupMappingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateRoleItemGroupMappingCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private UpdateRoleItemGroupMappingCommand BuildCommand(int id = 1) =>
            new() { Id = id, RoleId = 1, ItemGroupId = 10, IsActive = 1 };

        private void SetupHappyPath(UpdateRoleItemGroupMappingCommand command)
        {
            var existing = new UserManagement.Domain.Entities.RoleItemGroupMapping
            {
                Id = command.Id, RoleId = command.RoleId, ItemGroupId = command.ItemGroupId,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            var dto = new RoleItemGroupMappingDto { Id = command.Id, RoleId = command.RoleId };

            _mockQueryRepo.SetupSequence(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(existing)
                .ReturnsAsync(existing);
            _mockCommandRepo.Setup(r => r.CompositeKeyExistsAsync(command.RoleId, command.ItemGroupId, command.Id)).ReturnsAsync(false);
            _mockCommandRepo.Setup(r => r.UpdateAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.RoleItemGroupMapping>())).ReturnsAsync(1);
            _mockMapper
                .Setup(m => m.Map<RoleItemGroupMappingDto>(It.IsAny<UserManagement.Domain.Entities.RoleItemGroupMapping>()))
                .Returns(new RoleItemGroupMappingDto { Id = command.Id, RoleId = command.RoleId });
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            var command = BuildCommand();
            SetupHappyPath(command);
            var result = await CreateSut().Handle(command, CancellationToken.None);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            var command = BuildCommand();
            SetupHappyPath(command);
            await CreateSut().Handle(command, CancellationToken.None);
            _mockCommandRepo.Verify(r => r.UpdateAsync(1, It.IsAny<UserManagement.Domain.Entities.RoleItemGroupMapping>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = BuildCommand();
            SetupHappyPath(command);
            await CreateSut().Handle(command, CancellationToken.None);
            _mockMediator.Verify(m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Update" && e.Module == "RoleItemGroupMapping"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NotFoundId_ThrowsValidationException()
        {
            var command = BuildCommand(id: 999);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((UserManagement.Domain.Entities.RoleItemGroupMapping?)null);
            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<ValidationException>().WithMessage("*does not exist or is deleted*");
        }
    }
}
