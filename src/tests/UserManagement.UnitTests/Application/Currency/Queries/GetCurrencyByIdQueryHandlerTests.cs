using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.ICurrency;
using UserManagement.Application.Currency.Queries.GetCurrency;
using UserManagement.Application.Currency.Queries.GetCurrencyById;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.Currency.Queries
{
    public sealed class GetCurrencyByIdQueryHandlerTests
    {
        private readonly Mock<ICurrencyQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<GetCurrencyByIdQueryHandler>> _mockLogger = new();

        private GetCurrencyByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            // Arrange
            var entity = CurrencyBuilders.ValidEntity(id: 1);
            var dto = CurrencyBuilders.ValidDto(id: 1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<CurrencyDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetCurrencyByIdQuery { CurrencyId = 1 },
                CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Code.Should().Be("USD");
        }

        [Fact]
        public async Task Handle_NonExistentId_ThrowsValidationException()
        {
            // Arrange
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((UserManagement.Domain.Entities.Currency?)null);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(
                new GetCurrencyByIdQuery { CurrencyId = 999 },
                CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            // Arrange
            var entity = CurrencyBuilders.ValidEntity(id: 1);
            var dto = CurrencyBuilders.ValidDto(id: 1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<CurrencyDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(
                new GetCurrencyByIdQuery { CurrencyId = 1 },
                CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetCurrencyByIdQuery" &&
                        e.Module == "Currency"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
