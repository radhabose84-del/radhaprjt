using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BackgroundService.Presentation.Controllerss;
using BackgroundService.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete;
using BackgroundService.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById;

namespace BackgroundService.UnitTests.Controllers
{
    public sealed class MiscTypeMasterControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private MiscTypeMasterController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAllMiscTypeMasterAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscTypeMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetMiscTypeMasterDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetMiscTypeMasterDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllMiscTypeMasterAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllMiscTypeMasterAsync_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscTypeMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetMiscTypeMasterDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetMiscTypeMasterDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllMiscTypeMasterAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetMiscTypeMasterQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WhenSuccess_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscTypeMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<GetMiscTypeMasterDto>
                {
                    IsSuccess = true,
                    Data = new GetMiscTypeMasterDto(),
                    Message = "Success"
                });

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ReturnsNotFoundResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscTypeMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<GetMiscTypeMasterDto>
                {
                    IsSuccess = false,
                    Message = "Not found"
                });

            var result = await CreateSut().GetByIdAsync(999);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetMiscTypeMaster_AutoComplete_WhenSuccess_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscTypeMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetMiscTypeMasterAutocompleteDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetMiscTypeMasterAutocompleteDto>()
                });

            var result = await CreateSut().GetMiscTypeMaster("test");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_WhenSuccess_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateMiscTypeMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<GetMiscTypeMasterDto>
                {
                    IsSuccess = true,
                    Data = new GetMiscTypeMasterDto(),
                    Message = "Created"
                });

            var result = await CreateSut().CreateAsync(new CreateMiscTypeMasterCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_WhenSuccess_ReturnsOkResult()
        {
            // Update first calls GetById, then Send(update)
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscTypeMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<GetMiscTypeMasterDto>
                {
                    IsSuccess = true,
                    Data = new GetMiscTypeMasterDto()
                });

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateMiscTypeMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool>
                {
                    IsSuccess = true,
                    Message = "Updated"
                });

            var result = await CreateSut().Update(new UpdateMiscTypeMasterCommand { Id = 1 });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_WhenSuccess_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteMiscTypeMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<GetMiscTypeMasterDto>
                {
                    IsSuccess = true,
                    Message = "Deleted"
                });

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteMiscTypeMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<GetMiscTypeMasterDto>
                {
                    IsSuccess = true,
                    Message = "Deleted"
                });

            await CreateSut().Delete(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteMiscTypeMasterCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
