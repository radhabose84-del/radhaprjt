using Contracts.Common;
using FAM.Application.AssetMaster.AssetDisposal.Command.CreateAssetDisposal;
using FAM.Application.AssetMaster.AssetDisposal.Command.UpdateAssetDisposal;
using FAM.Application.AssetMaster.AssetDisposal.Queries.GetAssetDisposal;
using FAM.Application.AssetMaster.AssetDisposal.Queries.GetAssetDisposalById;
using FAM.Presentation.Controllers.AssetMaster;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FixedAssetManagement.UnitTests.Controllers
{
    public sealed class AssetDisposalControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<AssetDisposalController>> _mockLogger = new(MockBehavior.Loose);

        private AssetDisposalController CreateSut() =>
            new(_mockLogger.Object, _mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetDisposalQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetDisposalDto>>
                {
                    IsSuccess = true,
                    Data = new List<AssetDisposalDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllAssetDisposalAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetDisposalQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetDisposalDto>>
                {
                    IsSuccess = true,
                    Data = new List<AssetDisposalDto>()
                });

            await CreateSut().GetAllAssetDisposalAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAssetDisposalQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetDisposalByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(AssetDisposalBuilders.ValidDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateAssetDisposalCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(AssetDisposalBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateAssetDisposalCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().CreateAsync(AssetDisposalBuilders.ValidCreateCommand());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateAssetDisposalCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateAssetDisposalCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().UpdateAsync(AssetDisposalBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
