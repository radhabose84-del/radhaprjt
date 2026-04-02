using ProductionManagement.Application.Common.Interfaces.ICertificationMaster;
using ProductionManagement.Application.CertificationMaster.Queries.GetCertificationMasterAutoComplete;

namespace ProductionManagement.UnitTests.Application.CertificationMaster.Queries
{
    public sealed class GetCertificationMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<ICertificationMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetCertificationMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsLookupList()
        {
            var lookup = new List<CertificationMasterLookupDto> { new() { Id = 1 } };
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("iso", It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<CertificationMasterLookupDto>)lookup);
            _mockMapper.Setup(m => m.Map<List<CertificationMasterLookupDto>>(It.IsAny<object>()))
                .Returns(lookup);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetCertificationMasterAutoCompleteQuery("iso"), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsAll()
        {
            var lookup = new List<CertificationMasterLookupDto>();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<CertificationMasterLookupDto>)lookup);
            _mockMapper.Setup(m => m.Map<List<CertificationMasterLookupDto>>(It.IsAny<object>())).Returns(lookup);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetCertificationMasterAutoCompleteQuery(null), CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
