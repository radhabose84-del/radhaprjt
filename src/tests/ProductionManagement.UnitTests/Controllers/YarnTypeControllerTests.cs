using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.YarnType.Commands.CreateYarnType;
using ProductionManagement.Application.YarnType.Commands.UpdateYarnType;
using ProductionManagement.Application.YarnType.Commands.DeleteYarnType;
using ProductionManagement.Application.YarnType.Queries.GetAllYarnType;
using ProductionManagement.Application.YarnType.Queries.GetYarnTypeById;
using ProductionManagement.Application.YarnType.Queries.GetYarnTypeAutoComplete;
using ProductionManagement.Application.YarnType.Dto;
using ProductionManagement.Presentation.Controllers;

namespace ProductionManagement.UnitTests.Controllers
{
    public sealed class YarnTypeControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private YarnTypeController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllYarnTypeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<YarnTypeDto>> { IsSuccess = true, Data = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 });

            var result = await CreateSut().GetAllYarnTypeAsync(1, 10, null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllYarnTypeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<YarnTypeDto>> { IsSuccess = true, Data = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 });

            await CreateSut().GetAllYarnTypeAsync(1, 10, null);

            _mockMediator.Verify(m => m.Send(It.IsAny<GetAllYarnTypeQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetYarnTypeByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new YarnTypeDto { Id = 1 });

            var result = await CreateSut().GetYarnTypeByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetYarnTypeAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<YarnTypeLookupDto>() as IReadOnlyList<YarnTypeLookupDto>);

            var result = await CreateSut().GetYarnTypeAutoCompleteAsync(null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateYarnTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreateYarnType(new CreateYarnTypeCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateYarnTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().UpdateYarnType(new UpdateYarnTypeCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteYarnTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteYarnType(1);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
