using ProductionManagement.Application.Common.Interfaces.IRepacking;
using ProductionManagement.Application.Repacking.Dto;
using ProductionManagement.Application.Repacking.Queries.GetRepackingAutoComplete;

namespace ProductionManagement.UnitTests.Application.Repacking.Queries
{
    public sealed class GetRepackingAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IRepackingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetRepackingAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var lookup = new List<RepackingLookupDto> { new() { Id = 1, RepackingNo = "R001" } };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<RepackingLookupDto>)lookup);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetRepackingAutoCompleteQuery("R"), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullTerm_ReturnsEmpty()
        {
            var lookup = new List<RepackingLookupDto>();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<RepackingLookupDto>)lookup);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetRepackingAutoCompleteQuery(null!), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var lookup = new List<RepackingLookupDto>();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<RepackingLookupDto>)lookup);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GetRepackingAutoCompleteQuery("R"), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
