using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using UserManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using UserManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using UserManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using UserManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete;
using UserManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class MiscTypeMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private MiscTypeMasterController CreateSut() =>
            new(_mockMediator.Object);

        // --- GetAllMiscTypeMasterAsync ---

        [Fact]
        public async Task GetAllMiscTypeMasterAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscTypeMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetMiscTypeMasterDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<GetMiscTypeMasterDto> { new GetMiscTypeMasterDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllMiscTypeMasterAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_Success_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscTypeMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<GetMiscTypeMasterDto>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new GetMiscTypeMasterDto()
                });

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_NotFound_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscTypeMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<GetMiscTypeMasterDto>
                {
                    IsSuccess = false,
                    Message = "Not found"
                });

            var result = await CreateSut().GetByIdAsync(99);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        // --- GetMiscTypeMaster (AutoComplete) ---

        [Fact]
        public async Task GetMiscTypeMaster_Success_ReturnsOkResult()
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
        public async Task GetMiscTypeMaster_NotFound_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscTypeMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetMiscTypeMasterAutocompleteDto>>
                {
                    IsSuccess = false,
                    Message = "Not found"
                });

            var result = await CreateSut().GetMiscTypeMaster("xyz");

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        // --- CreateAsync ---

        [Fact]
        public async Task CreateAsync_Success_ReturnsOkResult()
        {
            var command = new CreateMiscTypeMasterCommand();

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateMiscTypeMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<GetMiscTypeMasterDto>
                {
                    IsSuccess = true,
                    Message = "Created",
                    Data = new GetMiscTypeMasterDto()
                });

            var result = await CreateSut().CreateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_Failure_ReturnsBadRequest()
        {
            var command = new CreateMiscTypeMasterCommand();

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateMiscTypeMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<GetMiscTypeMasterDto>
                {
                    IsSuccess = false,
                    Message = "Failed"
                });

            var result = await CreateSut().CreateAsync(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // --- Update ---

        [Fact]
        public async Task Update_Success_ReturnsOkResult()
        {
            var command = new UpdateMiscTypeMasterCommand { Id = 1 };

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

            var result = await CreateSut().Update(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_NotFound_ReturnsNotFound()
        {
            var command = new UpdateMiscTypeMasterCommand { Id = 99 };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscTypeMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ApiResponseDTO<GetMiscTypeMasterDto>?)null);

            var result = await CreateSut().Update(command);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        // --- Delete ---

        [Fact]
        public async Task Delete_Success_ReturnsOkResult()
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
        public async Task Delete_Failure_ReturnsBadRequest()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteMiscTypeMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<GetMiscTypeMasterDto>
                {
                    IsSuccess = false,
                    Message = "Failed"
                });

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
