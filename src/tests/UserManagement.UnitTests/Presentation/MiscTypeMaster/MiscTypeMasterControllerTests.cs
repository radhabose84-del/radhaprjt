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

namespace UserManagement.UnitTests.Presentation.MiscTypeMaster
{
    public sealed class MiscTypeMasterControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private MiscTypeMasterController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
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
        public async Task GetById_Success_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscTypeMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<GetMiscTypeMasterDto> { IsSuccess = true, Data = new GetMiscTypeMasterDto() });

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_NotFound_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscTypeMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<GetMiscTypeMasterDto> { IsSuccess = false, Message = "Not found." });

            var result = await CreateSut().GetByIdAsync(999);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetMiscTypeMaster_Success_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscTypeMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetMiscTypeMasterAutocompleteDto>> { IsSuccess = true, Data = new List<GetMiscTypeMasterAutocompleteDto>() });

            var result = await CreateSut().GetMiscTypeMaster("test");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_Success_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateMiscTypeMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<GetMiscTypeMasterDto> { IsSuccess = true, Data = new GetMiscTypeMasterDto() });

            var result = await CreateSut().CreateAsync(new CreateMiscTypeMasterCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_Failure_ReturnsBadRequest()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateMiscTypeMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<GetMiscTypeMasterDto> { IsSuccess = false, Message = "Already exists." });

            var result = await CreateSut().CreateAsync(new CreateMiscTypeMasterCommand());

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Update_Success_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscTypeMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<GetMiscTypeMasterDto> { IsSuccess = true, Data = new GetMiscTypeMasterDto() });

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateMiscTypeMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool> { IsSuccess = true });

            var result = await CreateSut().Update(new UpdateMiscTypeMasterCommand { Id = 1 });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_Success_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteMiscTypeMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<GetMiscTypeMasterDto> { IsSuccess = true, Data = new GetMiscTypeMasterDto() });

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
