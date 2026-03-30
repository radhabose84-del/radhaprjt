using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
using WarehouseManagement.Application.RackMaster.Command.CreateRackMaster;
using WarehouseManagement.Application.RackMaster.Command.DeleteRackMaster;
using WarehouseManagement.Application.RackMaster.Command.UpdateRackMaster;
using WarehouseManagement.Application.RackMaster.Queries.GetAllRackMaster;
using WarehouseManagement.Application.RackMaster.Queries.GetRackMasterAutoComplete;
using WarehouseManagement.Application.RackMaster.Queries.GetRackMasterById;
using WarehouseManagement.Presentation.Controllers;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Controllers
{
    public sealed class RackMasterControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IRackMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private RackMasterController CreateSut() => new(_mockMediator.Object, _mockQueryRepo.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllRackMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<RackMasterDto>>
                {
                    IsSuccess = true, Data = new List<RackMasterDto>(), TotalCount = 0, PageNumber = 1, PageSize = 10
                });

            var result = await CreateSut().GetAllWarehouseMasterAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetRackMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(RackMasterBuilders.ValidDto());

            var result = await CreateSut().GetByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateRackMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(RackMasterBuilders.ValidCreateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateRackMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().Update(RackMasterBuilders.ValidUpdateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteRackMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteWarehouseAsync(1, CancellationToken.None);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetRackMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(RackMasterBuilders.ValidAutoCompleteList());

            var result = await CreateSut().GetRackMaster("test", 0);
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
