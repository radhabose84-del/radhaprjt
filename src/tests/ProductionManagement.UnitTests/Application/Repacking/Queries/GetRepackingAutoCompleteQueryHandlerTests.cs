using ProductionManagement.Application.Common.Interfaces.IRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Dto;
using ProductionManagement.Application.RepackingHeader.Queries.GetRepackingHeaderAutoComplete;

namespace ProductionManagement.UnitTests.Application.Repacking.Queries
{
    public sealed class GetRepackingHeaderAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IRepackingHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetRepackingHeaderAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var lookups = new List<RepackingHeaderLookupDto>
            {
                new() { Id = 1, RepackDocNo = "REPACK-001", RepackDate = DateOnly.FromDateTime(DateTime.Today) }
            };
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("REPACK", It.IsAny<CancellationToken>(), null))
                .ReturnsAsync(lookups);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetRepackingHeaderAutoCompleteQuery("REPACK"),
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].RepackDocNo.Should().Be("REPACK-001");
        }

        [Fact]
        public async Task Handle_EmptyTerm_PassesEmptyString()
        {
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>(), null))
                .ReturnsAsync(new List<RepackingHeaderLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetRepackingHeaderAutoCompleteQuery(null!),
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var lookups = new List<RepackingHeaderLookupDto>
            {
                new() { Id = 1, RepackDocNo = "REPACK-001", RepackDate = DateOnly.FromDateTime(DateTime.Today) }
            };
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>(), null))
                .ReturnsAsync(lookups);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetRepackingHeaderAutoCompleteQuery("test"),
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "GetRepackingHeaderAutoCompleteQuery"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
