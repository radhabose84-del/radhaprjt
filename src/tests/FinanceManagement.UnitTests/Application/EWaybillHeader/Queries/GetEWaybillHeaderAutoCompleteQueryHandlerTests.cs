using FinanceManagement.Application.Common.Interfaces.IEWaybillHeader;
using FinanceManagement.Application.EWaybillHeader.Dto;
using FinanceManagement.Application.EWaybillHeader.Queries.GetEWaybillHeaderAutoComplete;

namespace FinanceManagement.UnitTests.Application.EWaybillHeader.Queries
{
    public sealed class GetEWaybillHeaderAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IEWaybillHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetEWaybillHeaderAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsMatchingResults()
        {
            var lookupList = new List<EWaybillHeaderLookupDto>
            {
                new() { Id = 1, EWBNumber = "EWB001", InvoiceNo = "INV001", EwbStatus = "Generated" }
            };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("EWB", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);
            _mockMapper
                .Setup(m => m.Map<List<EWaybillHeaderLookupDto>>(lookupList))
                .Returns(lookupList);

            var result = await CreateSut().Handle(
                new GetEWaybillHeaderAutoCompleteQuery("EWB"), CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].EWBNumber.Should().Be("EWB001");
        }

        [Fact]
        public async Task Handle_NullTerm_PassesEmptyString()
        {
            var emptyList = new List<EWaybillHeaderLookupDto>();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyList);
            _mockMapper
                .Setup(m => m.Map<List<EWaybillHeaderLookupDto>>(emptyList))
                .Returns(emptyList);

            var result = await CreateSut().Handle(
                new GetEWaybillHeaderAutoCompleteQuery(null), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var lookupList = new List<EWaybillHeaderLookupDto>();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);
            _mockMapper
                .Setup(m => m.Map<List<EWaybillHeaderLookupDto>>(lookupList))
                .Returns(lookupList);

            await CreateSut().Handle(
                new GetEWaybillHeaderAutoCompleteQuery("test"), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
