using ProductionManagement.Application.Common.Interfaces.IMiscTypeMaster;
using ProductionManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete;
using ProductionManagement.Application.MiscTypeMaster.Dto;

namespace ProductionManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsLookupList()
        {
            var lookup = new List<MiscTypeMasterLookupDto> { new() { Id = 1 } };
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<MiscTypeMasterLookupDto>)lookup);
            _mockMapper.Setup(m => m.Map<List<MiscTypeMasterLookupDto>>(It.IsAny<object>())).Returns(lookup);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetMiscTypeMasterAutoCompleteQuery("test"), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullTerm_ReturnsEmpty()
        {
            var lookup = new List<MiscTypeMasterLookupDto>();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<MiscTypeMasterLookupDto>)lookup);
            _mockMapper.Setup(m => m.Map<List<MiscTypeMasterLookupDto>>(It.IsAny<object>())).Returns(lookup);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetMiscTypeMasterAutoCompleteQuery(null), CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
