using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.ICurrency;
using UserManagement.Application.Currency.Commands.DeleteCurrency;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.Currency.Commands
{
    public sealed class DeleteCurrencyCommandHandlerTests
    {
        private readonly Mock<ICurrencyCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ICurrencyQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<DeleteCurrencyCommandHandler>> _mockLogger = new();

        private DeleteCurrencyCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object, _mockQueryRepo.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsId()
        {
            // Arrange
            var command = CurrencyBuilders.ValidDeleteCommand(id: 5);
            var existingEntity = CurrencyBuilders.ValidEntity(id: 5);
            var mappedEntity = CurrencyBuilders.ValidEntity(id: 5);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(existingEntity);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Currency>(command))
                .Returns(mappedEntity);

            _mockCommandRepo
                .Setup(r => r.DeletecurrencyAsync(5, It.IsAny<UserManagement.Domain.Entities.Currency>()))
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(5);
        }

        [Fact]
        public async Task Handle_CurrencyNotFound_ThrowsValidationException()
        {
            // Arrange
            var command = CurrencyBuilders.ValidDeleteCommand(id: 999);

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
        public async Task Handle_DeleteReturnsMinusOne_ThrowsValidationException()
        {
            // Arrange
            var command = CurrencyBuilders.ValidDeleteCommand(id: 5);
            var existingEntity = CurrencyBuilders.ValidEntity(id: 5);
            var mappedEntity = CurrencyBuilders.ValidEntity(id: 5);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(existingEntity);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Currency>(command))
                .Returns(mappedEntity);

            _mockCommandRepo
                .Setup(r => r.DeletecurrencyAsync(5, It.IsAny<UserManagement.Domain.Entities.Currency>()))
                .ReturnsAsync(-1);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            // Arrange
            var command = CurrencyBuilders.ValidDeleteCommand(id: 5);
            var existingEntity = CurrencyBuilders.ValidEntity(id: 5);
            var mappedEntity = CurrencyBuilders.ValidEntity(id: 5);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(existingEntity);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Currency>(command))
                .Returns(mappedEntity);

            _mockCommandRepo
                .Setup(r => r.DeletecurrencyAsync(5, It.IsAny<UserManagement.Domain.Entities.Currency>()))
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
                        e.ActionDetail == "Delete" &&
                        e.Module == "Currency"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
