using Contracts.Common;
using FAM.Application.UOM.Command.CreateUOM;
using FAM.Application.UOM.Command.DeleteUOM;
using FAM.Application.UOM.Command.UpdateUOM;
using FAM.Application.UOM.Queries.GetUOMAutoComplete;
using FAM.Application.UOM.Queries.GetUOMById;
using FAM.Application.UOM.Queries.GetUOMs;
using FAM.Presentation.Controllers;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FixedAssetManagement.UnitTests.Controllers
{
    public sealed class UOMControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private UOMController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUOMQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<UOMDto>>
                {
                    IsSuccess = true,
                    Data = new List<UOMDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllUOMAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUOMQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<UOMDto>>
                {
                    IsSuccess = true,
                    Data = new List<UOMDto>()
                });

            await CreateSut().GetAllUOMAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetUOMQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUOMByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(FAMUOMBuilders.ValidDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_InvalidId_ReturnsBadRequest()
        {
            var result = await CreateSut().GetByIdAsync(0);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUOMAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMAutoCompleteDto> { FAMUOMBuilders.ValidAutoCompleteDto() });

            var result = await CreateSut().GetUOM("kg");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateUOMCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(FAMUOMBuilders.ValidDto());

            var result = await CreateSut().CreateAsync(FAMUOMBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUOMByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(FAMUOMBuilders.ValidDto());

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateUOMCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Update(FAMUOMBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteUOMCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_InvalidId_ReturnsBadRequest()
        {
            var result = await CreateSut().Delete(0);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteUOMCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Delete(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteUOMCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
