using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.CountMaster.Commands.CreateCountMaster;
using ProductionManagement.Application.CountMaster.Commands.UpdateCountMaster;
using ProductionManagement.Application.CountMaster.Commands.DeleteCountMaster;
using ProductionManagement.Application.CountMaster.Queries.GetAllCountMaster;
using ProductionManagement.Application.CountMaster.Queries.GetCountMasterById;
using ProductionManagement.Application.CountMaster.Queries.GetCountMasterAutoComplete;
using ProductionManagement.Application.CountMaster.Dto;
using ProductionManagement.Presentation.Controllers;
using AppLookupDto = Contracts.Dtos.Lookups.Production.CountMasterLookupDto;

namespace ProductionManagement.UnitTests.Controllers
{
    public sealed class CountMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private CountMasterController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllCountMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<CountMasterDto>> { IsSuccess = true, Data = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 });

            var result = await CreateSut().GetAllCountMasterAsync(1, 10, null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllCountMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<CountMasterDto>> { IsSuccess = true, Data = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 });

            await CreateSut().GetAllCountMasterAsync(1, 10, null);

            _mockMediator.Verify(m => m.Send(It.IsAny<GetAllCountMasterQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetCountMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CountMasterDto { Id = 1 });

            var result = await CreateSut().GetCountMasterByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetCountMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IReadOnlyList<AppLookupDto>>(new List<AppLookupDto>()));

            var result = await CreateSut().GetCountMasterAutoCompleteAsync(null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateCountMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreateCountMaster(new CreateCountMasterCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateCountMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().UpdateCountMaster(new UpdateCountMasterCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteCountMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteCountMaster(1);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
