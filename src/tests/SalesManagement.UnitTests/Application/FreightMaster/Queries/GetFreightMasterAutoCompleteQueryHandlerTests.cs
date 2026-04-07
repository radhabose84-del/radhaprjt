using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IFreightMaster;
using SalesManagement.Application.FreightMaster.Dto;
using SalesManagement.Application.FreightMaster.Queries.GetFreightMasterAutoComplete;

namespace SalesManagement.UnitTests.Application.FreightMaster.Queries
{
    public class GetFreightMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IFreightMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetFreightMasterAutoCompleteQueryHandler CreateSut()
        {
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetFreightMasterAutoCompleteQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_WithTerm_ReturnsLookupList()
        {
            var lookupList = new List<FreightMasterLookupDto>
            {
                new() { Id = 1, FreightModeName = "Inner", Rate = 100m },
                new() { Id = 2, FreightModeName = "Outer", Rate = 200m }
            } as IReadOnlyList<FreightMasterLookupDto>;

            _mockQueryRepo.Setup(r => r.AutocompleteAsync("freight", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var result = await CreateSut().Handle(
                new GetFreightMasterAutoCompleteQuery("freight"), CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_WithTerm_CallsAutocompleteAsync_Once()
        {
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FreightMasterLookupDto>() as IReadOnlyList<FreightMasterLookupDto>);

            await CreateSut().Handle(
                new GetFreightMasterAutoCompleteQuery("test"), CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsEmptyList()
        {
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FreightMasterLookupDto>() as IReadOnlyList<FreightMasterLookupDto>);

            var result = await CreateSut().Handle(
                new GetFreightMasterAutoCompleteQuery(string.Empty), CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
