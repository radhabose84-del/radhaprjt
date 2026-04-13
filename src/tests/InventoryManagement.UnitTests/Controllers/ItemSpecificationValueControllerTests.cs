using Contracts.Common;
using InventoryManagement.Application.ItemSpecificationValue.Commands.CreateItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Commands.DeleteItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Commands.UpdateItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Dto;
using InventoryManagement.Application.ItemSpecificationValue.Queries.GetAllItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Queries.GetItemSpecificationValueAutoComplete;
using InventoryManagement.Application.ItemSpecificationValue.Queries.GetItemSpecificationValueById;
using InventoryManagement.Presentation.Controllers;
using InventoryManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.UnitTests.Controllers
{
    public sealed class ItemSpecificationValueControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private ItemSpecificationValueController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllItemSpecificationValueQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<ItemSpecificationValueDto>>
                {
                    IsSuccess = true,
                    Data = new List<ItemSpecificationValueDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllItemSpecificationValueAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllItemSpecificationValueQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<ItemSpecificationValueDto>>
                {
                    IsSuccess = true,
                    Data = new List<ItemSpecificationValueDto>()
                });

            await CreateSut().GetAllItemSpecificationValueAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllItemSpecificationValueQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetItemSpecificationValueByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ItemSpecificationValueBuilders.ValidDto());

            var result = await CreateSut().GetItemSpecificationValueByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetItemSpecificationValueAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ItemSpecificationValueLookupDto>)new List<ItemSpecificationValueLookupDto>
                {
                    ItemSpecificationValueBuilders.ValidLookupDto()
                });

            var result = await CreateSut().GetItemSpecificationValueAutoCompleteAsync("Red");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_WithSpecificationMasterId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetItemSpecificationValueAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ItemSpecificationValueLookupDto>)new List<ItemSpecificationValueLookupDto>
                {
                    ItemSpecificationValueBuilders.ValidLookupDto()
                });

            var result = await CreateSut().GetItemSpecificationValueAutoCompleteAsync("Red", specificationMasterId: 1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateItemSpecificationValueCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int>
                {
                    IsSuccess = true,
                    Data = 1
                });

            var result = await CreateSut().CreateItemSpecificationValue(
                ItemSpecificationValueBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateItemSpecificationValueCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int>
                {
                    IsSuccess = true,
                    Data = 1
                });

            var result = await CreateSut().UpdateItemSpecificationValue(
                ItemSpecificationValueBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteItemSpecificationValueCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteItemSpecificationValue(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteItemSpecificationValueCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteItemSpecificationValue(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteItemSpecificationValueCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
