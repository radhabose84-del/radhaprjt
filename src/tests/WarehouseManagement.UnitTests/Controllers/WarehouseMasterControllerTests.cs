using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Command.CreateWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Command.DeleteWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Command.UpdateWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.GetAllWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.GetWarehouseMasterById;
using WarehouseManagement.Application.WarehouseMaster.Queries.GetWareMasterAutoComplete;
using WarehouseManagement.Presentation.Controllers;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Controllers
{
    public sealed class WarehouseMasterControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IWarehouseMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private WarehouseMasterController CreateSut() => new(_mockMediator.Object, _mockQueryRepo.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllWarehouseMastersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<WarehouseMasterDto>>
                {
                    IsSuccess = true,
                    Data = new List<WarehouseMasterDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllWarehouseMasterAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetWarehouseMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<WarehouseMasterDto>
                {
                    IsSuccess = true,
                    Data = WarehouseMasterBuilders.ValidDto()
                });

            var result = await CreateSut().GetByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateWarehouseMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(WarehouseMasterBuilders.ValidCreateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateWarehouseMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool> { IsSuccess = true, Message = "Updated" });

            var result = await CreateSut().Update(WarehouseMasterBuilders.ValidUpdateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteWarehouseMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteWarehouseAsync(1, CancellationToken.None);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetWarehouseMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(WarehouseMasterBuilders.ValidAutoCompleteList());

            var result = await CreateSut().GetRackMaster("test");
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
