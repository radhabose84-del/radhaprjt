using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.ICurrency;
using UserManagement.Application.Currency.Queries.GetCurrency;
using UserManagement.Application.Currency.Queries.GetCurrencyAutoComplete;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.Currency.Queries
{
    public sealed class GetCurrencyAutoCompleteQueryHandlerTests
    {
        private readonly Mock<ICurrencyQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<GetCurrencyAutocompleteQueryHandler>> _mockLogger = new();

        private GetCurrencyAutocompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsList()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.Currency> { CurrencyBuilders.ValidEntity() };
            var dtos = new List<CurrencyAutoCompleteDto> { CurrencyBuilders.ValidAutoCompleteDto() };

            _mockQueryRepo
                .Setup(r => r.GetByCurrencyNameAsync("USD"))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<CurrencyAutoCompleteDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetCurrencyAutocompleteQuery { SearchPattern = "USD" },
                CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            result[0].Code.Should().Be("USD");
        }

        [Fact]
        public async Task Handle_NoResults_ThrowsValidationException()
        {
            // Arrange
            var emptyList = new List<UserManagement.Domain.Entities.Currency>();

            _mockQueryRepo
                .Setup(r => r.GetByCurrencyNameAsync("NONEXISTENT"))
                .ReturnsAsync(emptyList);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(
                new GetCurrencyAutocompleteQuery { SearchPattern = "NONEXISTENT" },
                CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*No currency found*");
        }

        [Fact]
        public async Task Handle_WithResults_PublishesAuditEvent()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.Currency> { CurrencyBuilders.ValidEntity() };
            var dtos = new List<CurrencyAutoCompleteDto> { CurrencyBuilders.ValidAutoCompleteDto() };

            _mockQueryRepo
                .Setup(r => r.GetByCurrencyNameAsync("USD"))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<CurrencyAutoCompleteDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(
                new GetCurrencyAutocompleteQuery { SearchPattern = "USD" },
                CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetCurrencyAutocompleteQuery" &&
                        e.Module == "Currency"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
