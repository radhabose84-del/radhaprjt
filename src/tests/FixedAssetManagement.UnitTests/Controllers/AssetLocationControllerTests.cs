using Contracts.Common;
using FAM.Application.AssetLocation.Commands.CreateAssetLocation;
using FAM.Application.AssetLocation.Queries.GetAssetLocation;
using FAM.Application.AssetLocation.Queries.GetAssetLocationById;
using FAM.Presentation.Controllers.AssetMaster;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FixedAssetManagement.UnitTests.Controllers
{
    public sealed class AssetLocationControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<AssetLocationController>> _mockLogger = new(MockBehavior.Loose);

        private AssetLocationController CreateSut() =>
            new(_mockLogger.Object, _mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetLocationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetLocationDto>>
                {
                    IsSuccess = true,
                    Data = new List<AssetLocationDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllAssetLocationAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetLocationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetLocationDto>>
                {
                    IsSuccess = true,
                    Data = new List<AssetLocationDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllAssetLocationAsync(1, 10);

            _mockMediator.Verify(m => m.Send(It.IsAny<GetAssetLocationQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            var dto = AssetLocationTestBuilders.ValidDto(1);

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetLocationByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            var dto = AssetLocationTestBuilders.ValidDto(1);

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateAssetLocationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var command = AssetLocationTestBuilders.ValidCreateCommand();
            var result = await CreateSut().CreateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
