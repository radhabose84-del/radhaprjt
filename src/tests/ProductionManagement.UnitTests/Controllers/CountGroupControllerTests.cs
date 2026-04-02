using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.CountGroup.Commands.CreateCountGroup;
using ProductionManagement.Application.CountGroup.Commands.UpdateCountGroup;
using ProductionManagement.Application.CountGroup.Commands.DeleteCountGroup;
using ProductionManagement.Application.CountGroup.Queries.GetAllCountGroup;
using ProductionManagement.Application.CountGroup.Queries.GetCountGroupById;
using ProductionManagement.Application.CountGroup.Queries.GetCountGroupAutoComplete;
using ProductionManagement.Application.CountGroup.Dto;
using ProductionManagement.Presentation.Controllers;

namespace ProductionManagement.UnitTests.Controllers
{
    public sealed class CountGroupControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private CountGroupController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllCountGroupQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<CountGroupDto>> { IsSuccess = true, Data = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 });

            var result = await CreateSut().GetAllCountGroupAsync(1, 10, null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllCountGroupQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<CountGroupDto>> { IsSuccess = true, Data = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 });

            await CreateSut().GetAllCountGroupAsync(1, 10, null);

            _mockMediator.Verify(m => m.Send(It.IsAny<GetAllCountGroupQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetCountGroupByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CountGroupDto { Id = 1 });

            var result = await CreateSut().GetCountGroupByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetCountGroupAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CountGroupLookupDto>() as IReadOnlyList<CountGroupLookupDto>);

            var result = await CreateSut().GetCountGroupAutoCompleteAsync(null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateCountGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreateCountGroup(new CreateCountGroupCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateCountGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().UpdateCountGroup(new UpdateCountGroupCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteCountGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteCountGroup(1);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
