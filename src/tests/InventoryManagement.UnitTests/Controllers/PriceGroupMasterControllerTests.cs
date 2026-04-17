using Contracts.Common;
using Contracts.Dtos.Lookups.Inventory;
using InventoryManagement.Application.PriceGroupMaster.Commands.CreatePriceGroupMaster;
using InventoryManagement.Application.PriceGroupMaster.Commands.DeletePriceGroupMaster;
using InventoryManagement.Application.PriceGroupMaster.Commands.UpdatePriceGroupMaster;
using InventoryManagement.Application.PriceGroupMaster.Dto;
using InventoryManagement.Application.PriceGroupMaster.Queries.GetAllPriceGroupMaster;
using InventoryManagement.Application.PriceGroupMaster.Queries.GetPriceGroupMasterAutoComplete;
using InventoryManagement.Application.PriceGroupMaster.Queries.GetPriceGroupMasterById;
using InventoryManagement.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.UnitTests.Controllers
{
    public sealed class PriceGroupMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private PriceGroupMasterController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllPriceGroupMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<PriceGroupMasterDto>>
                {
                    IsSuccess = true,
                    Data = new List<PriceGroupMasterDto>(),
                    TotalCount = 0, PageNumber = 1, PageSize = 10
                });

            var result = await CreateSut().GetAllPriceGroupMasterAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllPriceGroupMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<PriceGroupMasterDto>>
                {
                    IsSuccess = true, Data = new List<PriceGroupMasterDto>(),
                    TotalCount = 0, PageNumber = 1, PageSize = 10
                });

            await CreateSut().GetAllPriceGroupMasterAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllPriceGroupMasterQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPriceGroupMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PriceGroupMasterDto { Id = 1, PriceGroupCode = "PG1" });

            var result = await CreateSut().GetPriceGroupMasterByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPriceGroupMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<PriceGroupMasterLookupDto>)new List<PriceGroupMasterLookupDto>());

            var result = await CreateSut().GetPriceGroupMasterAutoCompleteAsync("STD");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreatePriceGroupMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int>
                {
                    IsSuccess = true, Message = "Price Group created successfully.", Data = 1
                });

            var result = await CreateSut().CreatePriceGroupMaster(new CreatePriceGroupMasterCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdatePriceGroupMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int>
                {
                    IsSuccess = true, Message = "Price Group updated successfully.", Data = 1
                });

            var result = await CreateSut().UpdatePriceGroupMaster(new UpdatePriceGroupMasterCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeletePriceGroupMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeletePriceGroupMaster(5);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeletePriceGroupMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeletePriceGroupMaster(5);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeletePriceGroupMasterCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
