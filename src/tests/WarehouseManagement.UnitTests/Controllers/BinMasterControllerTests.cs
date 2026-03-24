using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.Application.BinMaster.Command.CreateBinMaster;
using WarehouseManagement.Application.BinMaster.Command.DeleteBinMaster;
using WarehouseManagement.Application.BinMaster.Command.UpdateBinMaster;
using WarehouseManagement.Application.BinMaster.Queries.GetAllBinMaster;
using WarehouseManagement.Application.BinMaster.Queries.GetBinMasterAutoComplete;
using WarehouseManagement.Application.BinMaster.Queries.GetBinMasterById;
using WarehouseManagement.Application.Common.Interfaces.IBinMaster;
using WarehouseManagement.Presentation.Controllers;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Controllers
{
    public sealed class BinMasterControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IBinMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private BinMasterController CreateSut() => new(_mockMediator.Object, _mockQueryRepo.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllBinMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<BinMasterDto>>
                {
                    IsSuccess = true, Data = new List<BinMasterDto>(), TotalCount = 0, PageNumber = 1, PageSize = 10
                });

            var result = await CreateSut().GetAllBinMasterAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetBinMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(BinMasterBuilders.ValidDto());

            var result = await CreateSut().GetByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateBinMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(BinMasterBuilders.ValidCreateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateBinMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().Update(BinMasterBuilders.ValidUpdateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteBinMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteWarehouseAsync(1, CancellationToken.None);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_WithResults_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetBinMasterAutoComplete>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(BinMasterBuilders.ValidAutoCompleteList());

            var result = await CreateSut().GetBinMaster("test", 10, null, null, CancellationToken.None);
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
