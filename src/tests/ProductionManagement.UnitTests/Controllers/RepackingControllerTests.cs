using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.Repacking.Commands.CreateRepacking;
using ProductionManagement.Application.Repacking.Commands.UpdateRepacking;
using ProductionManagement.Application.Repacking.Commands.DeleteRepacking;
using ProductionManagement.Application.Repacking.Queries.GetAllRepacking;
using ProductionManagement.Application.Repacking.Queries.GetRepackingById;
using ProductionManagement.Application.Repacking.Queries.GetRepackingAutoComplete;
using ProductionManagement.Application.Repacking.Dto;
using ProductionManagement.Presentation.Controllers;

namespace ProductionManagement.UnitTests.Controllers
{
    public sealed class RepackingControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private RepackingController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllRepackingQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<RepackingHeaderDto>> { IsSuccess = true, Data = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 });

            var result = await CreateSut().GetAllRepackingAsync(1, 10, null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetRepackingByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepackingHeaderDto { Id = 1 });

            var result = await CreateSut().GetRepackingByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetRepackingAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RepackingLookupDto>() as IReadOnlyList<RepackingLookupDto>);

            var result = await CreateSut().GetRepackingAutoCompleteAsync(null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateRepackingCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreateRepacking(new CreateRepackingCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateRepackingCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().UpdateRepacking(new UpdateRepackingCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteRepackingCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteRepacking(1);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
