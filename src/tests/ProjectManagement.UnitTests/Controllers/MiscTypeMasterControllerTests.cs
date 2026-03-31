using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using ProjectManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using ProjectManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using ProjectManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using ProjectManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete;
using ProjectManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById;
using ProjectManagement.Presentation.Controllers;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Controllers
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
        public async Task GetAll_CallsMediatorSend_Once()
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
        public async Task GetById_EntityFound_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscTypeMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<GetMiscTypeMasterDto>
                {
                    IsSuccess = true,
                    Data = MiscTypeMasterBuilders.ValidDto()
                });

            var result = await CreateSut().GetByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_EntityNotFound_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscTypeMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<GetMiscTypeMasterDto>
                {
                    IsSuccess = false,
                    Data = null
                });

            var result = await CreateSut().GetByIdAsync(999);
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Create_ValidCommand_ReturnsOkResult()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateMiscTypeMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<GetMiscTypeMasterDto>
                {
                    IsSuccess = true,
                    Message = "Created",
                    Data = MiscTypeMasterBuilders.ValidDto()
                });

            var result = await CreateSut().CreateAsync(command);
            result.Should().BeOfType<OkObjectResult>();
        }

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
    }
}
