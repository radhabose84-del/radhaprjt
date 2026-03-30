using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IFinancialYear;
using UserManagement.Application.FinancialYear.Command.UpdateFinancialYear;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.FinancialYear.Commands
{
    public sealed class UpdateFinancialYearCommandHandlerTests
    {
        private readonly Mock<IFinancialYearCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<UpdateFinancialYearCommandHandler>> _mockLogger = new();

        private UpdateFinancialYearCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockLogger.Object, _mockMediator.Object);

        private static UpdateFinancialYearCommand ValidCommand(int id = 1) => new UpdateFinancialYearCommand
        {
            Id = id,
            StartYear = "2025-26",
            StartDate = new DateTime(2025, 4, 1),
            EndDate = new DateTime(2026, 3, 31),
            FinYearName = "FY2025-26",
            IsActive = 1
        };

        [Fact]
        public async Task Handle_ValidCommand_ReturnsPositiveResult()
        {
            // Arrange
            var command = ValidCommand();
            var entity = new UserManagement.Domain.Entities.FinancialYear
            {
                Id = 1,
                StartDate = command.StartDate
            };

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.FinancialYear>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(1, entity))
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
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            // Arrange
            var command = ValidCommand(id: 999);
            var entity = new UserManagement.Domain.Entities.FinancialYear { Id = 999 };

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.FinancialYear>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(999, entity))
                .ReturnsAsync(0);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            // Arrange
            var command = ValidCommand();
            var entity = new UserManagement.Domain.Entities.FinancialYear { Id = 1 };

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.FinancialYear>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(1, entity))
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockCommandRepo.Verify(r => r.UpdateAsync(1, It.IsAny<UserManagement.Domain.Entities.FinancialYear>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            // Arrange
            var command = ValidCommand();
            var entity = new UserManagement.Domain.Entities.FinancialYear
            {
                Id = 1,
                StartDate = command.StartDate
            };

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.FinancialYear>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(1, entity))
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
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Update"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
