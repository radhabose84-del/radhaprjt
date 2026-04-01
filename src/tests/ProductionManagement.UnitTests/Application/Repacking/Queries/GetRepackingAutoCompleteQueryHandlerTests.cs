using ProductionManagement.Application.Common.Interfaces.IRepackingMaster;
using ProductionManagement.Application.RepackingMaster.Dto;
using ProductionManagement.Application.RepackingMaster.Queries.GetRepackingMasterAutoComplete;

namespace ProductionManagement.UnitTests.Application.Repacking.Queries
{
    public sealed class GetRepackingMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IRepackingMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetRepackingMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var lookups = new List<RepackingMasterLookupDto>
            {
                new() { Id = 1, RepackDocNo = "REPACK-001", RepackDate = DateOnly.FromDateTime(DateTime.Today) }
            };
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("REPACK", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetRepackingMasterAutoCompleteQuery("REPACK"),
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].RepackDocNo.Should().Be("REPACK-001");
        }

        [Fact]
        public async Task Handle_EmptyTerm_PassesEmptyString()
        {
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RepackingMasterLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetRepackingMasterAutoCompleteQuery(null!),
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var lookups = new List<RepackingMasterLookupDto>
            {
                new() { Id = 1, RepackDocNo = "REPACK-001", RepackDate = DateOnly.FromDateTime(DateTime.Today) }
            };
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetRepackingMasterAutoCompleteQuery("test"),
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "GetRepackingMasterAutoCompleteQuery"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
