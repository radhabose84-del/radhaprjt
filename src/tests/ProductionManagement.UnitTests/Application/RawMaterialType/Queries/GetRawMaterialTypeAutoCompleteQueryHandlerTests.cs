using Contracts.Dtos.Lookups.Production;
using ProductionManagement.Application.Common.Interfaces.IRawMaterialType;
using ProductionManagement.Application.RawMaterialType.Queries.GetRawMaterialTypeAutoComplete;

namespace ProductionManagement.UnitTests.Application.RawMaterialType.Queries
{
    public sealed class GetRawMaterialTypeAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IRawMaterialTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetRawMaterialTypeAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsMappedLookupList()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("term", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RawMaterialTypeLookupDto>());
            _mockMapper
                .Setup(m => m.Map<List<RawMaterialTypeLookupDto>>(It.IsAny<IReadOnlyList<RawMaterialTypeLookupDto>>()))
                .Returns(new List<RawMaterialTypeLookupDto> { new() { Id = 1 } });

            var result = await CreateSut().Handle(new GetRawMaterialTypeAutoCompleteQuery("term"), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullTerm_PassesEmptyStringToRepo()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RawMaterialTypeLookupDto>());
            _mockMapper
                .Setup(m => m.Map<List<RawMaterialTypeLookupDto>>(It.IsAny<IReadOnlyList<RawMaterialTypeLookupDto>>()))
                .Returns(new List<RawMaterialTypeLookupDto>());

            await CreateSut().Handle(new GetRawMaterialTypeAutoCompleteQuery(null), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("x", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RawMaterialTypeLookupDto>());
            _mockMapper
                .Setup(m => m.Map<List<RawMaterialTypeLookupDto>>(It.IsAny<IReadOnlyList<RawMaterialTypeLookupDto>>()))
                .Returns(new List<RawMaterialTypeLookupDto>());

            await CreateSut().Handle(new GetRawMaterialTypeAutoCompleteQuery("x"), CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
