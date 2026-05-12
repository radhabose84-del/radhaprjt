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

        // -----------------------------------------------------------------------------
        // Regression for Bug #2 — every success response from this controller was
        // returning ApiResponseDTO with IsSuccess defaulting to false despite a 200/201
        // status code. The frontend treated these as failures. These tests pin the
        // contract that ALL success paths must set IsSuccess = true.
        // -----------------------------------------------------------------------------

        private static ApiResponseDTO<T> ExtractOkBody<T>(ActionResult<ApiResponseDTO<T>> result)
        {
            var ok = result.Result as OkObjectResult;
            ok.Should().NotBeNull();
            return ok!.Value as ApiResponseDTO<T> ?? throw new InvalidOperationException("Body was not ApiResponseDTO<T>.");
        }

        private static ApiResponseDTO<T> ExtractCreatedBody<T>(ActionResult<ApiResponseDTO<T>> result)
        {
            var created = result.Result as CreatedAtActionResult;
            created.Should().NotBeNull();
            return created!.Value as ApiResponseDTO<T> ?? throw new InvalidOperationException("Body was not ApiResponseDTO<T>.");
        }

        [Fact]
        public async Task GetAll_Success_SetsIsSuccessTrue()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllItemsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<ItemListDto>(), 0));

            var body = ExtractOkBody(await CreateSut().GetAll());

            body.IsSuccess.Should().BeTrue();
            body.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetById_Success_SetsIsSuccessTrue()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetItemByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ItemDetailsDto());

            var body = ExtractOkBody(await CreateSut().GetById(1));

            body.IsSuccess.Should().BeTrue();
            body.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Create_Success_SetsIsSuccessTrueOnCreatedResponse()
        {
            var payload = new ItemDto { Id = 0 };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateItemCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(42);

            var body = ExtractCreatedBody(await CreateSut().Create(payload, CancellationToken.None));

            body.IsSuccess.Should().BeTrue();
            body.StatusCode.Should().Be(201);
            body.Data.Should().Be(42);
        }

        [Fact]
        public async Task Update_Success_SetsIsSuccessTrue()
        {
            var payload = new ItemDto { Id = 5 };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateItemCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MediatR.Unit.Value);

            var body = ExtractOkBody(await CreateSut().Update(payload, CancellationToken.None));

            body.IsSuccess.Should().BeTrue();
            body.StatusCode.Should().Be(200);
            body.Data.Should().Be(5);
        }

        [Fact]
        public async Task GetAutoComplete_Success_SetsIsSuccessTrue()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetItemAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetItemAutoCompleteDto>());

            var body = ExtractOkBody(await CreateSut().GetAutoComplete(null, ct: CancellationToken.None));

            body.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task GetItemsByVariantFilter_Success_SetsIsSuccessTrue()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetItemsByVariantFilterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetItemAutoCompleteDto>());

            var body = ExtractOkBody(await CreateSut().GetItemsByVariantFilter(ct: CancellationToken.None));

            body.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task CreateVariants_Success_SetsIsSuccessTrue()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateItemVariantsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<int> { 1 });

            var body = ExtractOkBody(await CreateSut().CreateVariants(new CreateItemVariantsCommand(), CancellationToken.None));

            body.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task CreateTemplate_Success_SetsIsSuccessTrueOnCreatedResponse()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateItemTemplateCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(99);

            var body = ExtractCreatedBody(await CreateSut().CreateTemplate(new CreateItemTemplateCommand(), CancellationToken.None));

            body.IsSuccess.Should().BeTrue();
            body.StatusCode.Should().Be(201);
            body.Data.Should().Be(99);
        }

        [Fact]
        public async Task DeleteLogo_Success_SetsIsSuccessTrue()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteFileCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var body = ExtractOkBody(await CreateSut().DeleteLogo(new DeleteFileCommand(), CancellationToken.None));

            body.IsSuccess.Should().BeTrue();
            body.Data.Should().BeTrue();
        }

        [Fact]
        public async Task GetLogById_Success_SetsIsSuccessTrue()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetItemLogByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ItemLogDto());

            var body = ExtractOkBody(await CreateSut().GetLogById(1, CancellationToken.None));

            body.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task GetByIds_EmptyList_SetsIsSuccessTrue()
        {
            var body = ExtractOkBody(await CreateSut().GetByIds(new List<int>(), CancellationToken.None));

            body.IsSuccess.Should().BeTrue();
            body.Data.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public async Task GetByIds_NonEmptyList_SetsIsSuccessTrue()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetItemsMasterByIdsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetItemAutoCompleteDto>());

            var body = ExtractOkBody(await CreateSut().GetByIds(new List<int> { 1, 2 }, CancellationToken.None));

            body.IsSuccess.Should().BeTrue();
        }
    }
}
