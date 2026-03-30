using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IHSNMaster;
using InventoryManagement.Application.HSNMaster.Command.CreateHSNMaster;
using InventoryManagement.Application.HSNMaster.Command.DeleteHSNMaster;
using InventoryManagement.Application.HSNMaster.Command.UpdateHSNMaster;
using InventoryManagement.Application.HSNMaster.Queries.GetAllHSNMaster;
using InventoryManagement.Application.HSNMaster.Queries.GetHSNMasterAutoComplete;
using InventoryManagement.Application.HSNMaster.Queries.GetHSNMasterById;
using InventoryManagement.Presentation.Controllers;
using InventoryManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.UnitTests.Controllers
{
    public sealed class HSNMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IHSNMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private HSNMasterController CreateSut() =>
            new(_mockMediator.Object, _mockQueryRepo.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetHSNMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<HSNMasterDto>>
                {
                    IsSuccess = true,
                    Data = new List<HSNMasterDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllHSNMasterAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetHSNMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<HSNMasterDto>
                {
                    IsSuccess = true,
                    Data = HSNMasterBuilders.ValidDto()
                });

            var result = await CreateSut().GetHSNMasterByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetHSNMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetHSNMasterAutoCompleteDto>());

            var result = await CreateSut().GetHSNMasterAutoCompleteAsync();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateHSNMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int>
                {
                    IsSuccess = true,
                    Data = 1
                });

            var result = await CreateSut().CreateHSNMaster(HSNMasterBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteHSNMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool>
                {
                    IsSuccess = true,
                    Data = true
                });

            var result = await CreateSut().DeleteHSNMaster(1);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
