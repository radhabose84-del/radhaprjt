using MediatR;
using QCManagement.Application.Common.Interfaces.IQualityTemplate;
using QCManagement.Application.QualityTemplate.Queries.GetQualityTemplateAutoComplete;
using QCManagement.Domain.Events;
using QCManagement.UnitTests.TestData;

namespace QCManagement.UnitTests.Application.QualityTemplate.Queries
{
    public class GetQualityTemplateAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IQualityTemplateQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private GetQualityTemplateAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsLookupList()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("yarn", It.IsAny<CancellationToken>()))
                .ReturnsAsync(QualityTemplateBuilders.ValidLookupList());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetQualityTemplateAutoCompleteQuery("yarn"), CancellationToken.None);

            result.Should().NotBeEmpty();
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsAll()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(QualityTemplateBuilders.ValidLookupList());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetQualityTemplateAutoCompleteQuery(string.Empty), CancellationToken.None);

            result.Should().HaveCount(1);
        }
    }
}
