using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Commands.CreateRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Dto;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.RoleItemGroupMapping.Commands
{
    public sealed class CreateRoleItemGroupMappingCommandHandlerTests
    {
        private readonly Mock<IRoleItemGroupMappingCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateRoleItemGroupMappingCommandHandler CreateSut() =>
            new(_mockMapper.Object, _mockCommandRepo.Object, _mockMediator.Object);

        private static CreateRoleItemGroupMappingCommand BuildCommand(int roleId = 1, int itemGroupId = 10) =>
            new() { RoleId = roleId, ItemGroupId = itemGroupId };

        private void SetupHappyPath(CreateRoleItemGroupMappingCommand command)
        {
            var entity = new UserManagement.Domain.Entities.RoleItemGroupMapping
            {
                Id = 1, RoleId = command.RoleId, ItemGroupId = command.ItemGroupId
            };

            _mockCommandRepo
                .Setup(r => r.CompositeKeyExistsAsync(command.RoleId, command.ItemGroupId, null))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.RoleItemGroupMapping>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.RoleItemGroupMapping>()))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<RoleItemGroupMappingDto>(entity))
                .Returns(new RoleItemGroupMappingDto { Id = 1, RoleId = command.RoleId, ItemGroupId = command.ItemGroupId });

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            var command = BuildCommand();
            SetupHappyPath(command);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.RoleId.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var command = BuildCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.RoleItemGroupMapping>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = BuildCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.Module == "RoleItemGroupMapping"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DuplicateCompositeKey_ThrowsValidationException()
        {
            var command = BuildCommand(roleId: 2, itemGroupId: 20);

            _mockCommandRepo
                .Setup(r => r.CompositeKeyExistsAsync(2, 20, null))
                .ReturnsAsync(true);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*already exists*");
        }
    }
}
