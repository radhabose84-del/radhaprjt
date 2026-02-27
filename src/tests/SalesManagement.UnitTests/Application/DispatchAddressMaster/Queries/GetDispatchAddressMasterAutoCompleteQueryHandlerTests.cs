using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMaster;
using SalesManagement.Application.DispatchAddressMaster.Dto;
using SalesManagement.Application.DispatchAddressMaster.Queries.GetDispatchAddressMasterAutoComplete;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.DispatchAddressMaster.Queries
{
    public sealed class GetDispatchAddressMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IDispatchAddressMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetDispatchAddressMasterAutoCompleteQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<List<DispatchAddressMasterLookupDto>>(It.IsAny<object>()))
                .Returns<object>(o => o as List<DispatchAddressMasterLookupDto> ?? new List<DispatchAddressMasterLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetDispatchAddressMasterAutoCompleteQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_WithTerm_ReturnsMatchingResults()
        {
            var lookupList = DispatchAddressMasterBuilders.ValidLookupList().ToList();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("Test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var result = await CreateSut().Handle(
                new GetDispatchAddressMasterAutoCompleteQuery("Test"),
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DispatchAddressMasterLookupDto>());

            var result = await CreateSut().Handle(
                new GetDispatchAddressMasterAutoCompleteQuery(null!),
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEventOnce()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DispatchAddressMasterLookupDto>());

            await CreateSut().Handle(
                new GetDispatchAddressMasterAutoCompleteQuery("addr"),
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
