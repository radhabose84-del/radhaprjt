using ProductionManagement.Application.Common.Interfaces.IYarnConversionHeader;
using ProductionManagement.Application.YarnConversionHeader.Dto;
using ProductionManagement.Application.YarnConversionHeader.Queries.GetYarnConversionHeaderAutoComplete;

namespace ProductionManagement.UnitTests.Application.YarnConversionHeader.Queries
{
    public sealed class GetYarnConversionHeaderAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IYarnConversionHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetYarnConversionHeaderAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var lookups = new List<YarnConversionHeaderLookupDto>
            {
                new() { Id = 1, ConversionDocNo = "YC-001", ConversionDate = DateOnly.FromDateTime(DateTime.Today) }
            };
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("YC", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetYarnConversionHeaderAutoCompleteQuery("YC"),
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].ConversionDocNo.Should().Be("YC-001");
        }

        [Fact]
        public async Task Handle_EmptyTerm_PassesEmptyString()
        {
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<YarnConversionHeaderLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetYarnConversionHeaderAutoCompleteQuery(null!),
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var lookups = new List<YarnConversionHeaderLookupDto>
            {
                new() { Id = 1, ConversionDocNo = "YC-001", ConversionDate = DateOnly.FromDateTime(DateTime.Today) }
            };
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetYarnConversionHeaderAutoCompleteQuery("test"),
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "GetYarnConversionHeaderAutoCompleteQuery"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
