using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.ICurrency;
using UserManagement.Application.Currency.Commands.UpdateCurrency;
using UserManagement.Application.Currency.Queries.GetCurrency;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.Currency.Commands
{
    public sealed class UpdateCurrencyCommandHandlerTests
    {
        private readonly Mock<ICurrencyCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ICurrencyQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<UpdateCurrencyCommandHandler>> _mockLogger = new();

        private UpdateCurrencyCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockLogger.Object, _mockMediator.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(UpdateCurrencyCommand command)
        {
            var existingEntity = CurrencyBuilders.ValidEntity(id: command.Id);
            var mappedEntity = CurrencyBuilders.ValidEntity(id: command.Id);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(existingEntity);

            _mockCommandRepo
                .Setup(r => r.ExistsByNameupdateAsync(command.Name!, command.Id))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Currency>(command))
                .Returns(mappedEntity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.Currency>()))
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsResult()
        {
            // Arrange
            var command = CurrencyBuilders.ValidUpdateCommand();
            SetupHappyPath(command);
            var sut = CreateSut();

            // Act
            var result = await sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_CurrencyNotFound_ThrowsValidationException()
        {
            // Arrange
            var command = CurrencyBuilders.ValidUpdateCommand(id: 999);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((UserManagement.Domain.Entities.Currency?)null);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_DuplicateName_ThrowsValidationException()
        {
            // Arrange
            var command = CurrencyBuilders.ValidUpdateCommand();
            var existingEntity = CurrencyBuilders.ValidEntity(id: command.Id);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(existingEntity);

            _mockCommandRepo
                .Setup(r => r.ExistsByNameupdateAsync(command.Name!, command.Id))
                .ReturnsAsync(true);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task Handle_UpdateReturnsMinusOne_ThrowsValidationException()
        {
            // Arrange
            var command = CurrencyBuilders.ValidUpdateCommand();
            var existingEntity = CurrencyBuilders.ValidEntity(id: command.Id);
            var mappedEntity = CurrencyBuilders.ValidEntity(id: command.Id);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(existingEntity);

            _mockCommandRepo
                .Setup(r => r.ExistsByNameupdateAsync(command.Name!, command.Id))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Currency>(command))
                .Returns(mappedEntity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.Currency>()))
                .ReturnsAsync(-1);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            // Arrange
            var command = CurrencyBuilders.ValidUpdateCommand();
            SetupHappyPath(command);
            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.Module == "Currency"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
