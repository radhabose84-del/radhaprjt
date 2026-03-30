using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Commands.DeleteRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Dto;
using UserManagement.Domain.Enums.Common;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.RoleItemGroupMapping.Commands
{
    public sealed class DeleteRoleItemGroupMappingCommandHandlerTests
    {
        private readonly Mock<IRoleItemGroupMappingCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IRoleItemGroupMappingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteRoleItemGroupMappingCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int id)
        {
            var existing = new UserManagement.Domain.Entities.RoleItemGroupMapping
            {
                Id = id, RoleId = 1, ItemGroupId = 10, IsDeleted = Enums.IsDelete.NotDeleted
            };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
            _mockCommandRepo.Setup(r => r.DeleteAsync(id, It.IsAny<UserManagement.Domain.Entities.RoleItemGroupMapping>())).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            SetupHappyPath(1);
            var result = await CreateSut().Handle(new DeleteRoleItemGroupMappingCommand { Id = 1 }, CancellationToken.None);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsDeleteOnce()
        {
            SetupHappyPath(2);
            await CreateSut().Handle(new DeleteRoleItemGroupMappingCommand { Id = 2 }, CancellationToken.None);
            _mockCommandRepo.Verify(r => r.DeleteAsync(2, It.IsAny<UserManagement.Domain.Entities.RoleItemGroupMapping>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            SetupHappyPath(3);
            await CreateSut().Handle(new DeleteRoleItemGroupMappingCommand { Id = 3 }, CancellationToken.None);
            _mockMediator.Verify(m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Delete" && e.Module == "RoleItemGroupMapping"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NotFoundId_ThrowsValidationException()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((UserManagement.Domain.Entities.RoleItemGroupMapping?)null);
            Func<Task> act = async () => await CreateSut().Handle(new DeleteRoleItemGroupMappingCommand { Id = 999 }, CancellationToken.None);
            await act.Should().ThrowAsync<ValidationException>().WithMessage("*does not exist or is deleted*");
        }
    }
}
