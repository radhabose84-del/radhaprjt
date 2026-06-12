using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IIconMaster;
using UserManagement.Application.IconMaster.Commands.CreateIconMaster;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.IconMaster.Commands
{
    public sealed class CreateIconMasterCommandHandlerTests
    {
        private readonly Mock<IIconMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<CreateIconMasterCommandHandler>> _mockLogger = new();

        private CreateIconMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        private void SetupHappyPath(CreateIconMasterCommand command, int newId = 1)
        {
            _mockCommandRepo.Setup(r => r.ExistsByKeywordAsync(command.Keyword!)).ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.IconMaster>(command))
                .Returns(IconMasterBuilders.ValidEntity());
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.IconMaster>()))
                .ReturnsAsync(newId);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            var command = IconMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 5);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(5);
        }

        [Fact]
        public async Task Handle_DuplicateKeyword_ThrowsValidationException()
        {
            var command = IconMasterBuilders.ValidCreateCommand();
            _mockCommandRepo.Setup(r => r.ExistsByKeywordAsync(command.Keyword!)).ReturnsAsync(true);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*already exists*");
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsException()
        {
            var command = IconMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 0);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>().WithMessage("IconMaster Creation Failed");
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = IconMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Create" && e.Module == "IconMaster"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var command = IconMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.IconMaster>()), Times.Once);
        }
    }
}
