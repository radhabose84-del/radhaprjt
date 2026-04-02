using ProductionManagement.Application.Common.Interfaces.IMiscMaster;
using ProductionManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;
using ProductionManagement.Application.MiscMaster.Dto;

namespace ProductionManagement.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetMiscMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsLookupList()
        {
            var lookup = new List<MiscMasterLookupDto> { new() { Id = 1 } };
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<MiscMasterLookupDto>)lookup);
            _mockMapper.Setup(m => m.Map<List<MiscMasterLookupDto>>(It.IsAny<object>())).Returns(lookup);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetMiscMasterAutoCompleteQuery("test", null), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullTerm_ReturnsEmpty()
        {
            var lookup = new List<MiscMasterLookupDto>();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<MiscMasterLookupDto>)lookup);
            _mockMapper.Setup(m => m.Map<List<MiscMasterLookupDto>>(It.IsAny<object>())).Returns(lookup);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetMiscMasterAutoCompleteQuery(null, null), CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
