using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BackgroundService.Presentation.Controllers;
using BackgroundService.Application.MiscMaster.Command.CreateMiscMaster;
using BackgroundService.Application.MiscMaster.Command.DeleteMiscMaster;
using BackgroundService.Application.MiscMaster.Command.UpdateMiscMaster;
using BackgroundService.Application.MiscMaster.Queries.GetMiscMaster;
using BackgroundService.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;
using BackgroundService.Application.MiscMaster.Queries.GetMiscMasterById;

namespace BackgroundService.UnitTests.Controllers
{
    public sealed class MiscMasterControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private MiscMasterController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAllMiscMasterAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetMiscMasterDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetMiscMasterDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllMiscMasterAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllMiscMasterAsync_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetMiscMasterDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetMiscMasterDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllMiscMasterAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetMiscMasterQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetMiscMasterDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetMiscMaster_AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetMiscMasterAutoCompleteDto>());

            var result = await CreateSut().GetMiscMaster("test", "TYPE01");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateMiscMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetMiscMasterDto());

            var result = await CreateSut().CreateAsync(new CreateMiscMasterCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateMiscMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Update(new UpdateMiscMasterCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteMiscMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteMiscMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Delete(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteMiscMasterCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
