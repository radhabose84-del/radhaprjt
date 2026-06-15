using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IIconMaster;
using UserManagement.Application.IconMaster.Commands.UpdateIconMaster;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.IconMaster.Commands
{
    public sealed class UpdateIconMasterCommandHandlerTests
    {
        private readonly Mock<IIconMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IIconMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<UpdateIconMasterCommandHandler>> _mockLogger = new();

        private UpdateIconMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockLogger.Object, _mockMediator.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(UpdateIconMasterCommand command)
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(IconMasterBuilders.ValidEntity(id: command.Id));
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.IconMaster>(command))
                .Returns(IconMasterBuilders.ValidEntity(id: command.Id));
            _mockCommandRepo.Setup(r => r.UpdateAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.IconMaster>()))
                .ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsResult()
        {
            var command = IconMasterBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            var command = IconMasterBuilders.ValidUpdateCommand(id: 999);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((UserManagement.Domain.Entities.IconMaster?)null);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_UpdateReturnsMinusOne_ThrowsValidationException()
        {
            var command = IconMasterBuilders.ValidUpdateCommand();
            _mockQueryRepo.Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(IconMasterBuilders.ValidEntity(id: command.Id));
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.IconMaster>(command))
                .Returns(IconMasterBuilders.ValidEntity(id: command.Id));
            _mockCommandRepo.Setup(r => r.UpdateAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.IconMaster>()))
                .ReturnsAsync(-1);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = IconMasterBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Update" && e.Module == "IconMaster"),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
