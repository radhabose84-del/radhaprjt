using Contracts.Common;
using InventoryManagement.Application.Item.ItemDetail.Commands.CreateItem;
using InventoryManagement.Application.Item.ItemDetail.Commands.CreateItemTemplate;
using InventoryManagement.Application.Item.ItemDetail.Commands.CreateItemVariants;
using InventoryManagement.Application.Item.ItemDetail.Commands.DeleteItemImage;
using InventoryManagement.Application.Item.ItemDetail.Commands.UpdateItem;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemAutoComplete;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemById;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogById;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogs;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemsByIds;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemsByVariantFilter;
using InventoryManagement.Presentation.Controllers.Item;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.UnitTests.Controllers
{
    public sealed class ItemMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private ItemMasterController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllItemsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<ItemListDto>(), 0));

            var result = await CreateSut().GetAll();

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllItemsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<ItemListDto>(), 0));

            await CreateSut().GetAll();

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllItemsQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_WithExistingId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetItemByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ItemDetailsDto());

            var result = await CreateSut().GetById(1);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_WithNonExistingId_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetItemByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ItemDetailsDto?)null);

            var result = await CreateSut().GetById(999);

            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Create_WithValidPayload_ReturnsCreatedAtAction()
        {
            var payload = new ItemDto { Id = 0 };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateItemCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().Create(payload, CancellationToken.None);

            result.Result.Should().BeOfType<CreatedAtActionResult>();
        }

        [Fact]
        public async Task Create_WithNullPayload_ReturnsBadRequest()
        {
            var result = await CreateSut().Create(null!, CancellationToken.None);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetAutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetItemAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetItemAutoCompleteDto>());

            var result = await CreateSut().GetAutoComplete(null, ct: CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetItemsByVariantFilter_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetItemsByVariantFilterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetItemAutoCompleteDto>());

            var result = await CreateSut().GetItemsByVariantFilter(ct: CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateVariants_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateItemVariantsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<int> { 1 });

            var result = await CreateSut().CreateVariants(new CreateItemVariantsCommand(), CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteLogo_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteFileCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteLogo(new DeleteFileCommand(), CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetLogById_WithExistingId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetItemLogByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ItemLogDto());

            var result = await CreateSut().GetLogById(1, CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIds_WithEmptyList_ReturnsOkWithEmpty()
        {
            var result = await CreateSut().GetByIds(new List<int>(), CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIds_WithValidIds_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetItemsMasterByIdsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetItemAutoCompleteDto>());

            var result = await CreateSut().GetByIds(new List<int> { 1, 2 }, CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }
    }
}
