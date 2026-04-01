using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.LotMaster.Commands.CreateLotMaster;
using ProductionManagement.Application.LotMaster.Commands.UpdateLotMaster;
using ProductionManagement.Application.LotMaster.Commands.DeleteLotMaster;
using ProductionManagement.Application.LotMaster.Queries.GetAllLotMaster;
using ProductionManagement.Application.LotMaster.Queries.GetLotMasterById;
using ProductionManagement.Application.LotMaster.Queries.GetLotMasterAutoComplete;
using ProductionManagement.Application.LotMaster.Dto;
using ProductionManagement.Presentation.Controllers;
using AppLookupDto = Contracts.Dtos.Lookups.Production.LotMasterLookupDto;

namespace ProductionManagement.UnitTests.Controllers
{
    public sealed class LotMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private LotMasterController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllLotMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<LotMasterDto>> { IsSuccess = true, Data = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 });

            var result = await CreateSut().GetAllLotMasterAsync(1, 10, null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllLotMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<LotMasterDto>> { IsSuccess = true, Data = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 });

            await CreateSut().GetAllLotMasterAsync(1, 10, null);

            _mockMediator.Verify(m => m.Send(It.IsAny<GetAllLotMasterQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetLotMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LotMasterDto { Id = 1 });

            var result = await CreateSut().GetLotMasterByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetLotMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IReadOnlyList<AppLookupDto>>(new List<AppLookupDto>()));

            var result = await CreateSut().GetLotMasterAutoCompleteAsync(null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateLotMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreateLotMaster(new CreateLotMasterCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateLotMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().UpdateLotMaster(new UpdateLotMasterCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteLotMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteLotMaster(1);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
