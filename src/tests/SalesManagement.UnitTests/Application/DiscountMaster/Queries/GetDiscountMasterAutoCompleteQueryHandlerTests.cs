using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDiscountMaster;
using SalesManagement.Application.DiscountMaster.Dto;
using SalesManagement.Application.DiscountMaster.Queries.GetDiscountMasterAutoComplete;

namespace SalesManagement.UnitTests.Application.DiscountMaster.Queries
{
    public class GetDiscountMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IDiscountMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetDiscountMasterAutoCompleteQueryHandler CreateSut()
        {
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetDiscountMasterAutoCompleteQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_WithTerm_ReturnsLookupList()
        {
            var lookupList = new List<DiscountMasterLookupDto>
            {
                new() { Id = 1, DiscountCode = "D001", DiscountName = "Discount A" },
                new() { Id = 2, DiscountCode = "D002", DiscountName = "Discount B" }
            } as IReadOnlyList<DiscountMasterLookupDto>;

            _mockQueryRepo.Setup(r => r.AutocompleteAsync("disc", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var result = await CreateSut().Handle(
                new GetDiscountMasterAutoCompleteQuery("disc"), CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_WithTerm_CallsAutocompleteAsync_Once()
        {
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DiscountMasterLookupDto>() as IReadOnlyList<DiscountMasterLookupDto>);

            await CreateSut().Handle(
                new GetDiscountMasterAutoCompleteQuery("test"), CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsEmptyList()
        {
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DiscountMasterLookupDto>() as IReadOnlyList<DiscountMasterLookupDto>);

            var result = await CreateSut().Handle(
                new GetDiscountMasterAutoCompleteQuery(string.Empty), CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
