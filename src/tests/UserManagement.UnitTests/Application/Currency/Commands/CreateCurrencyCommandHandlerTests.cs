using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.ICurrency;
using UserManagement.Application.Currency.Commands.CreateCurrency;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.Currency.Commands
{
    public sealed class CreateCurrencyCommandHandlerTests
    {
        private readonly Mock<ICurrencyCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<CreateCurrencyCommandHandler>> _mockLogger = new();

        private CreateCurrencyCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            // Arrange
            var command = CurrencyBuilders.ValidCreateCommand();
            var entity = CurrencyBuilders.ValidEntity();

            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(command.Code!))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Currency>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Currency>()))
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_DuplicateCode_ThrowsValidationException()
        {
            // Arrange
            var command = CurrencyBuilders.ValidCreateCommand();

            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(command.Code!))
                .ReturnsAsync(true);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsException()
        {
            // Arrange
            var command = CurrencyBuilders.ValidCreateCommand();
            var entity = CurrencyBuilders.ValidEntity();

            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(command.Code!))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Currency>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Currency>()))
                .ReturnsAsync(0);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Currency Creation Failed");
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            // Arrange
            var command = CurrencyBuilders.ValidCreateCommand();
            var entity = CurrencyBuilders.ValidEntity();

            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(command.Code!))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Currency>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Currency>()))
                .ReturnsAsync(1);

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
                        e.Module == "Currency"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            // Arrange
            var command = CurrencyBuilders.ValidCreateCommand();
            var entity = CurrencyBuilders.ValidEntity();

            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(command.Code!))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Currency>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Currency>()))
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Currency>()),
                Times.Once);
        }
    }
}
