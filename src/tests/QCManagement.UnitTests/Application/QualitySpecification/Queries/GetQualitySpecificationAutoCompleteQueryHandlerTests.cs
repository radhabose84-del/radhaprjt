using QCManagement.Application.Common.Interfaces.IQualitySpecification;
using QCManagement.Application.QualitySpecification.Dto;
using QCManagement.Application.QualitySpecification.Queries.GetQualitySpecificationAutoComplete;
using QCManagement.UnitTests.TestData;

namespace QCManagement.UnitTests.Application.QualitySpecification.Queries
{
    public class GetQualitySpecificationAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IQualitySpecificationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private GetQualitySpecificationAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsLookupList()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("cotton", It.IsAny<CancellationToken>()))
                .ReturnsAsync(QualitySpecificationBuilders.ValidLookupList());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetQualitySpecificationAutoCompleteQuery("cotton"), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsAll()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<QualitySpecificationLookupDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetQualitySpecificationAutoCompleteQuery(string.Empty), CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
