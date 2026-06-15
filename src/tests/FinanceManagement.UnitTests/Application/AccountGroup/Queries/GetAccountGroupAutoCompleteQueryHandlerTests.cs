using FinanceManagement.Application.AccountGroup.Dto;
using FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupAutoComplete;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;

namespace FinanceManagement.UnitTests.Application.AccountGroup.Queries
{
    public sealed class GetAccountGroupAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IAccountGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAccountGroupAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsMatches()
        {
            IReadOnlyList<AccountGroupLookupDto> matches = new List<AccountGroupLookupDto>
            {
                new() { Id = 1, GroupCode = "A-CA-INV", GroupName = "Inventories", Level = 3 }
            };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(matches);

            var result = await CreateSut().Handle(new GetAccountGroupAutoCompleteQuery("inv"), CancellationToken.None);

            result.Should().ContainSingle();
        }

        [Fact]
        public async Task Handle_NullTerm_PassesEmptyStringToRepository()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AccountGroupLookupDto>());

            await CreateSut().Handle(new GetAccountGroupAutoCompleteQuery(null), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AccountGroupLookupDto>());

            await CreateSut().Handle(new GetAccountGroupAutoCompleteQuery("x"), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
