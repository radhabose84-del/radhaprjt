using MediatR;
using PartyManagement.Application.BankAccount.Query.GetBankAutocomplete;
using PartyManagement.Application.Common.Interfaces.IBankAccount;
using Xunit;

namespace PartyManagement.UnitTests.Application.BankAccount.Queries
{
    public sealed class GetBankAccountAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IBankAccountQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetBankAccountAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsMatchingItems()
        {
            var items = new List<BankLookupDto>
            {
                new BankLookupDto { Id = 1, AccountNumber = "1234567890", BankName = "ICICI Bank" }
            };

            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("ICICI", It.IsAny<CancellationToken>()))
                .ReturnsAsync(items);

            var result = await CreateSut().Handle(
                new GetBankAccountAutoCompleteQuery("ICICI"),
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].BankName.Should().Be("ICICI Bank");
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsResults()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BankLookupDto>());

            var result = await CreateSut().Handle(
                new GetBankAccountAutoCompleteQuery(""),
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BankLookupDto>());

            await CreateSut().Handle(
                new GetBankAccountAutoCompleteQuery("test"),
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<PartyManagement.Domain.Events.AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
