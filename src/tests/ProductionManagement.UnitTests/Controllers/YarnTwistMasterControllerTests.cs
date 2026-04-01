using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.YarnTwistMaster.Commands.CreateYarnTwistMaster;
using ProductionManagement.Application.YarnTwistMaster.Commands.UpdateYarnTwistMaster;
using ProductionManagement.Application.YarnTwistMaster.Commands.DeleteYarnTwistMaster;
using ProductionManagement.Application.YarnTwistMaster.Queries.GetAllYarnTwistMaster;
using ProductionManagement.Application.YarnTwistMaster.Queries.GetYarnTwistMasterById;
using ProductionManagement.Application.YarnTwistMaster.Queries.GetYarnTwistMasterAutoComplete;
using ProductionManagement.Application.YarnTwistMaster.Dto;
using ProductionManagement.Presentation.Controllers;

namespace ProductionManagement.UnitTests.Controllers
{
    public sealed class YarnTwistMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private YarnTwistMasterController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllYarnTwistMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<YarnTwistMasterDto>> { IsSuccess = true, Data = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 });

            var result = await CreateSut().GetAllYarnTwistMasterAsync(1, 10, null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllYarnTwistMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<YarnTwistMasterDto>> { IsSuccess = true, Data = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 });

            await CreateSut().GetAllYarnTwistMasterAsync(1, 10, null);

            _mockMediator.Verify(m => m.Send(It.IsAny<GetAllYarnTwistMasterQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetYarnTwistMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new YarnTwistMasterDto { Id = 1 });

            var result = await CreateSut().GetYarnTwistMasterByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetYarnTwistMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<YarnTwistMasterLookupDto>() as IReadOnlyList<YarnTwistMasterLookupDto>);

            var result = await CreateSut().GetYarnTwistMasterAutoCompleteAsync(null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateYarnTwistMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreateYarnTwistMaster(new CreateYarnTwistMasterCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateYarnTwistMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().UpdateYarnTwistMaster(new UpdateYarnTwistMasterCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteYarnTwistMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteYarnTwistMaster(1);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
