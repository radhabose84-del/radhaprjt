using ProductionManagement.Application.Common.Interfaces.IProductionPack;
using ProductionManagement.Application.ProductionPack.Dto;
using ProductionManagement.Application.ProductionPack.Queries.GetProductionAutoComplete;

namespace ProductionManagement.UnitTests.Application.ProductionPack.Queries
{
    public sealed class GetProductionAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IProductionQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetProductionAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var lookup = new List<ProductionLookupDto> { new() { Id = 1, PackNo = "P001" } };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ProductionLookupDto>)lookup);
            _mockMapper
                .Setup(m => m.Map<List<ProductionLookupDto>>(It.IsAny<object>()))
                .Returns(lookup);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetProductionAutoCompleteQuery("P"), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullTerm_ReturnsResults()
        {
            var lookup = new List<ProductionLookupDto>();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ProductionLookupDto>)lookup);
            _mockMapper
                .Setup(m => m.Map<List<ProductionLookupDto>>(It.IsAny<object>()))
                .Returns(lookup);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetProductionAutoCompleteQuery(null!), CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
