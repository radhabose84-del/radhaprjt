using ProductionManagement.Application.Common.Interfaces.IProcessMaster;
using ProductionManagement.Application.ProcessMaster.Queries.GetProcessMasterAutoComplete;

namespace ProductionManagement.UnitTests.Application.ProcessMaster.Queries
{
    public sealed class GetProcessMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IProcessMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetProcessMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsLookupList()
        {
            var lookup = new List<ProcessMasterLookupDto> { new() { Id = 1 } };
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ProcessMasterLookupDto>)lookup);
            _mockMapper.Setup(m => m.Map<List<ProcessMasterLookupDto>>(It.IsAny<object>())).Returns(lookup);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetProcessMasterAutoCompleteQuery("test"), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullTerm_ReturnsEmptyList()
        {
            var lookup = new List<ProcessMasterLookupDto>();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ProcessMasterLookupDto>)lookup);
            _mockMapper.Setup(m => m.Map<List<ProcessMasterLookupDto>>(It.IsAny<object>())).Returns(lookup);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetProcessMasterAutoCompleteQuery(null), CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
