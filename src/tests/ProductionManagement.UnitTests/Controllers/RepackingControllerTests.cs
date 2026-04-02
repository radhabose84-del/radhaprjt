using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.RepackingMaster.Commands.CreateRepackingMaster;
using ProductionManagement.Application.RepackingMaster.Commands.DeleteRepackingMaster;
using ProductionManagement.Application.RepackingMaster.Commands.UpdateRepackingMaster;
using ProductionManagement.Application.RepackingMaster.Dto;
using ProductionManagement.Application.RepackingMaster.Queries.GetAllRepackingMaster;
using ProductionManagement.Application.RepackingMaster.Queries.GetRepackingMasterAutoComplete;
using ProductionManagement.Application.RepackingMaster.Queries.GetRepackingMasterById;
using ProductionManagement.Presentation.Controllers;

namespace ProductionManagement.UnitTests.Controllers
{
    public sealed class RepackingMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private RepackingMasterController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllRepackingMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<RepackingMasterDto>>
                {
                    IsSuccess = true,
                    Data = new List<RepackingMasterDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllRepackingMasterAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllRepackingMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<RepackingMasterDto>>
                {
                    IsSuccess = true,
                    Data = new List<RepackingMasterDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllRepackingMasterAsync(1, 10);
            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllRepackingMasterQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetRepackingMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepackingMasterDto { Id = 1 });

            var result = await CreateSut().GetRepackingMasterByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetRepackingMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RepackingMasterLookupDto>());

            var result = await CreateSut().GetRepackingMasterAutoCompleteAsync("test");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateRepackingMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Created", Data = 1 });

            var result = await CreateSut().CreateRepackingMaster(new CreateRepackingMasterCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateRepackingMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Updated", Data = 1 });

            var result = await CreateSut().UpdateRepackingMaster(new UpdateRepackingMasterCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteRepackingMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteRepackingMaster(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteRepackingMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteRepackingMaster(1);
            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteRepackingMasterCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
