using FinanceManagement.Application.Common.Interfaces.IDocumentSequence;
using FinanceManagement.Application.DocumentSequence.Dto;
using FinanceManagement.Application.DocumentSequence.Queries.GetDocumentSequenceAutoComplete;

namespace FinanceManagement.UnitTests.Application.DocumentSequence.Queries
{
    public sealed class GetDocumentSequenceAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IDocumentSequenceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetDocumentSequenceAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsMatchingResults()
        {
            var lookupList = new List<DocumentSequenceLookupDto>
            {
                new() { Id = 1, TypeName = "Invoice", DocNo = 100 }
            };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("Inv", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);
            _mockMapper
                .Setup(m => m.Map<List<DocumentSequenceLookupDto>>(lookupList))
                .Returns(lookupList);

            var result = await CreateSut().Handle(
                new GetDocumentSequenceAutoCompleteQuery("Inv"), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullTerm_PassesEmptyString()
        {
            var emptyList = new List<DocumentSequenceLookupDto>();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyList);
            _mockMapper
                .Setup(m => m.Map<List<DocumentSequenceLookupDto>>(emptyList))
                .Returns(emptyList);

            var result = await CreateSut().Handle(
                new GetDocumentSequenceAutoCompleteQuery(null), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var lookupList = new List<DocumentSequenceLookupDto>();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);
            _mockMapper
                .Setup(m => m.Map<List<DocumentSequenceLookupDto>>(lookupList))
                .Returns(lookupList);

            await CreateSut().Handle(
                new GetDocumentSequenceAutoCompleteQuery("test"), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
