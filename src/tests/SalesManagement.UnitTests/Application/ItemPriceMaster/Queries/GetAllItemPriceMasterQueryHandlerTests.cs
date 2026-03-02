using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Dto;
using SalesManagement.Application.ItemPriceMaster.Queries.GetAllItemPriceMaster;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.ItemPriceMaster.Queries
{
    public class GetAllItemPriceMasterQueryHandlerTests
    {
        private readonly Mock<IItemPriceMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetAllItemPriceMasterQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<List<ItemPriceMasterDto>>(It.IsAny<object>()))
                .Returns<object>(o => o as List<ItemPriceMasterDto> ?? new List<ItemPriceMasterDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetAllItemPriceMasterQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_ReturnsPagedResult_WithCorrectData()
        {
            var list = new List<SalesManagement.Application.ItemPriceMaster.Dto.ItemPriceMasterDto>
            {
                ItemPriceMasterBuilders.ValidDto(id: 1),
                ItemPriceMasterBuilders.ValidDto(id: 2, priceCode: "PC002")
            };

            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((list, 2));

            var query = new GetAllItemPriceMasterQuery { PageNumber = 1, PageSize = 10, SearchTerm = null };

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task Handle_EmptyList_ReturnsSuccessWithEmptyData()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<SalesManagement.Application.ItemPriceMaster.Dto.ItemPriceMasterDto>(), 0));

            var query = new GetAllItemPriceMasterQuery { PageNumber = 1, PageSize = 10, SearchTerm = null };

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_PassesSearchTermToRepository()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, "PC"))
                .ReturnsAsync((new List<SalesManagement.Application.ItemPriceMaster.Dto.ItemPriceMasterDto>(), 0));

            var query = new GetAllItemPriceMasterQuery { PageNumber = 1, PageSize = 10, SearchTerm = "PC" };

            await CreateSut().Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAllAsync(1, 10, "PC"), Times.Once);
        }

        [Fact]
        public async Task Handle_PageNumberAndSizeReflectedInResult()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(3, 5, null))
                .ReturnsAsync((new List<SalesManagement.Application.ItemPriceMaster.Dto.ItemPriceMasterDto>(), 0));

            var query = new GetAllItemPriceMasterQuery { PageNumber = 3, PageSize = 5, SearchTerm = null };

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.PageNumber.Should().Be(3);
            result.PageSize.Should().Be(5);
        }
    }
}
