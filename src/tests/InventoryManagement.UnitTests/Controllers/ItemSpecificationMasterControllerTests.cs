using Contracts.Common;
using InventoryManagement.Application.ItemSpecificationMaster.Commands.CreateItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Commands.DeleteItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Commands.UpdateItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Dto;
using InventoryManagement.Application.ItemSpecificationMaster.Queries.GetAllItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Queries.GetItemSpecificationMasterAutoComplete;
using InventoryManagement.Application.ItemSpecificationMaster.Queries.GetItemSpecificationMasterById;
using InventoryManagement.Presentation.Controllers;
using InventoryManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.UnitTests.Controllers
{
    public sealed class ItemSpecificationMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private ItemSpecificationMasterController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllItemSpecificationMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<ItemSpecificationMasterDto>>
                {
                    IsSuccess = true,
                    Data = new List<ItemSpecificationMasterDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllItemSpecificationMasterAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllItemSpecificationMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<ItemSpecificationMasterDto>>
                {
                    IsSuccess = true,
                    Data = new List<ItemSpecificationMasterDto>()
                });

            await CreateSut().GetAllItemSpecificationMasterAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllItemSpecificationMasterQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetItemSpecificationMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ItemSpecificationMasterBuilders.ValidDto());

            var result = await CreateSut().GetItemSpecificationMasterByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetItemSpecificationMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ItemSpecificationMasterLookupDto>)new List<ItemSpecificationMasterLookupDto>
                {
                    ItemSpecificationMasterBuilders.ValidLookupDto()
                });

            var result = await CreateSut().GetItemSpecificationMasterAutoCompleteAsync("Color");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateItemSpecificationMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int>
                {
                    IsSuccess = true,
                    Data = 1
                });

            var result = await CreateSut().CreateItemSpecificationMaster(
                ItemSpecificationMasterBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateItemSpecificationMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int>
                {
                    IsSuccess = true,
                    Data = 1
                });

            var result = await CreateSut().UpdateItemSpecificationMaster(
                ItemSpecificationMasterBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteItemSpecificationMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteItemSpecificationMaster(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteItemSpecificationMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteItemSpecificationMaster(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteItemSpecificationMasterCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
