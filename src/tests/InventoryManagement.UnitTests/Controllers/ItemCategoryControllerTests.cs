using Contracts.Common;
using InventoryManagement.Application.Item.ItemCategory.Commands.CreateItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.DeleteItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.UpdateItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategoryAutoComplete;
using InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategoryById;
using InventoryManagement.Presentation.Controllers.Item;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.UnitTests.Controllers
{
    public sealed class ItemCategoryControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private ItemCategoryController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetItemCategoryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<ItemCategoryDto>>
                {
                    IsSuccess = true, Data = new List<ItemCategoryDto>(), TotalCount = 0, PageNumber = 1, PageSize = 10
                });

            var result = await CreateSut().GetAllItemCategoryAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetItemCategoryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<ItemCategoryDto>> { IsSuccess = true, Data = new List<ItemCategoryDto>() });

            await CreateSut().GetAllItemCategoryAsync(1, 10);
            _mockMediator.Verify(m => m.Send(It.IsAny<GetItemCategoryQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetItemCategoryByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ItemCategoryDto { Id = 1 });

            var result = await CreateSut().GetByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetItemCategoryAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemCategoryAutoCompleteDto>());

            var result = await CreateSut().GetItemCategoryAutoCompleteAsync(null, null, false, 0);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateItemCategoryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(new CreateItemCategoryCommand { ItemCategoryName = "Test", ItemGroupId = 1 });
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateItemCategoryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().UpdateAsync(new UpdateItemCategoryCommand { Id = 1, ItemCategoryName = "Updated", ItemGroupId = 1 });
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteItemCategoryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().DeleteAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
