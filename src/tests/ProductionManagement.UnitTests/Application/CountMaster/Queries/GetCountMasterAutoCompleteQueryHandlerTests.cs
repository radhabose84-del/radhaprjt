using ProductionManagement.Application.Common.Interfaces.ICountMaster;
using ProductionManagement.Application.CountMaster.Queries.GetCountMasterAutoComplete;

namespace ProductionManagement.UnitTests.Application.CountMaster.Queries
{
    public sealed class GetCountMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<ICountMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetCountMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsLookupList()
        {
            var lookup = new List<CountMasterLookupDto> { new() { Id = 1 } };
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IReadOnlyList<CountMasterLookupDto>>(lookup));
            _mockMapper.Setup(m => m.Map<List<CountMasterLookupDto>>(It.IsAny<object>())).Returns(lookup);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetCountMasterAutoCompleteQuery("test"), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullTerm_ReturnsEmpty()
        {
            var lookup = new List<CountMasterLookupDto>();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IReadOnlyList<CountMasterLookupDto>>(lookup));
            _mockMapper.Setup(m => m.Map<List<CountMasterLookupDto>>(It.IsAny<object>())).Returns(lookup);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetCountMasterAutoCompleteQuery(null), CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
