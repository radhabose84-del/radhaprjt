using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IFinancialYear;
using UserManagement.Application.FinancialYear.Command.CreateFinancialYear;
using UserManagement.Application.FinancialYear.Queries.GetFinancialYear;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.FinancialYear.Commands
{
    public sealed class CreateFinancialYearCommandHandlerTests
    {
        private readonly Mock<IFinancialYearCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IFinancialYearQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreateFinancialYearCommandHandler>> _mockLogger = new();

        private CreateFinancialYearCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object, _mockQueryRepo.Object);

        private static CreateFinancialYearCommand ValidCommand() => new CreateFinancialYearCommand
        {
            StartYear = "2024-25",
            StartDate = new DateTime(2024, 4, 1),
            EndDate = new DateTime(2025, 3, 31),
            FinYearName = "FY2024-25"
        };

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            // Arrange
            var command = ValidCommand();
            var entity = new UserManagement.Domain.Entities.FinancialYear
            {
                Id = 1,
                StartYear = "2024-25",
                StartDate = command.StartDate,
                EndDate = command.EndDate
            };
            var dto = new FinancialYearDto { Id = 1, StartYear = "2024-25" };

            _mockQueryRepo
                .Setup(r => r.GetAllFinancialYearAsync(1, int.MaxValue, null))
                .ReturnsAsync((new List<UserManagement.Domain.Entities.FinancialYear>(), 0));

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.FinancialYear>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(entity))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<FinancialYearDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.StartYear.Should().Be("2024-25");
        }

        [Fact]
        public async Task Handle_DuplicateDateRange_ThrowsValidationException()
        {
            // Arrange
            var command = ValidCommand();
            var existing = new UserManagement.Domain.Entities.FinancialYear
            {
                Id = 1,
                StartDate = command.StartDate,
                EndDate = command.EndDate
            };

            _mockQueryRepo
                .Setup(r => r.GetAllFinancialYearAsync(1, int.MaxValue, null))
                .ReturnsAsync((new List<UserManagement.Domain.Entities.FinancialYear> { existing }, 1));

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task Handle_CreateReturnsNull_ThrowsException()
        {
            // Arrange
            var command = ValidCommand();
            var entity = new UserManagement.Domain.Entities.FinancialYear
            {
                Id = 0,
                StartYear = "2024-25",
                StartDate = command.StartDate,
                EndDate = command.EndDate
            };

            _mockQueryRepo
                .Setup(r => r.GetAllFinancialYearAsync(1, int.MaxValue, null))
                .ReturnsAsync((new List<UserManagement.Domain.Entities.FinancialYear>(), 0));

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.FinancialYear>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(entity))
                .ReturnsAsync((UserManagement.Domain.Entities.FinancialYear?)null!);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*not created*");
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            // Arrange
            var command = ValidCommand();
            var entity = new UserManagement.Domain.Entities.FinancialYear
            {
                Id = 5,
                StartYear = "2024-25",
                StartDate = command.StartDate,
                EndDate = command.EndDate
            };
            var dto = new FinancialYearDto { Id = 5, StartYear = "2024-25" };

            _mockQueryRepo
                .Setup(r => r.GetAllFinancialYearAsync(1, int.MaxValue, null))
                .ReturnsAsync((new List<UserManagement.Domain.Entities.FinancialYear>(), 0));

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.FinancialYear>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(entity))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<FinancialYearDto>(entity))
                .Returns(dto);

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
                        e.Module == "FinancialYear"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
