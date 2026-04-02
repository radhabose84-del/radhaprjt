using ProductionManagement.Application.Common.Interfaces.IYarnTwistMaster;
using ProductionManagement.Application.YarnTwistMaster.Queries.GetYarnTwistMasterAutoComplete;

namespace ProductionManagement.UnitTests.Application.YarnTwistMaster.Queries
{
    public sealed class GetYarnTwistMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IYarnTwistMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetYarnTwistMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsLookupList()
        {
            var lookup = new List<YarnTwistMasterLookupDto> { new() { Id = 1 } };
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<YarnTwistMasterLookupDto>)lookup);
            _mockMapper.Setup(m => m.Map<List<YarnTwistMasterLookupDto>>(It.IsAny<object>())).Returns(lookup);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetYarnTwistMasterAutoCompleteQuery("test"), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullTerm_ReturnsEmptyList()
        {
            var lookup = new List<YarnTwistMasterLookupDto>();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<YarnTwistMasterLookupDto>)lookup);
            _mockMapper.Setup(m => m.Map<List<YarnTwistMasterLookupDto>>(It.IsAny<object>())).Returns(lookup);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetYarnTwistMasterAutoCompleteQuery(null), CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
