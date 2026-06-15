using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IIconMaster;
using UserManagement.Application.IconMaster.Commands.DeleteIconMaster;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.IconMaster.Commands
{
    public sealed class DeleteIconMasterCommandHandlerTests
    {
        private readonly Mock<IIconMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IIconMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<DeleteIconMasterCommandHandler>> _mockLogger = new();

        private DeleteIconMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object, _mockQueryRepo.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsId()
        {
            var command = IconMasterBuilders.ValidDeleteCommand(id: 3);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(IconMasterBuilders.ValidEntity(id: 3));
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.IconMaster>(command))
                .Returns(IconMasterBuilders.ValidEntity(id: 3));
            _mockCommandRepo.Setup(r => r.DeleteIconMasterAsync(3, It.IsAny<UserManagement.Domain.Entities.IconMaster>()))
                .ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(3);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            var command = IconMasterBuilders.ValidDeleteCommand(id: 999);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((UserManagement.Domain.Entities.IconMaster?)null);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_DeleteReturnsMinusOne_ThrowsValidationException()
        {
            var command = IconMasterBuilders.ValidDeleteCommand(id: 4);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(IconMasterBuilders.ValidEntity(id: 4));
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.IconMaster>(command))
                .Returns(IconMasterBuilders.ValidEntity(id: 4));
            _mockCommandRepo.Setup(r => r.DeleteIconMasterAsync(4, It.IsAny<UserManagement.Domain.Entities.IconMaster>()))
                .ReturnsAsync(-1);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*not found*");
        }
    }
}
