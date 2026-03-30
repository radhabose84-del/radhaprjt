using Contracts.Common;
using FAM.Application.AssetGroup.Command.CreateAssetGroup;
using FAM.Application.AssetGroup.Command.DeleteAssetGroup;
using FAM.Application.AssetGroup.Command.UpdateAssetGroup;
using FAM.Application.AssetGroup.Queries.GetAssetGroup;
using FAM.Application.AssetGroup.Queries.GetAssetGroupAutoComplete;
using FAM.Application.AssetGroup.Queries.GetAssetGroupById;
using FAM.Presentation.Controllers;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FixedAssetManagement.UnitTests.Controllers
{
    public sealed class AssetGroupControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<AssetGroupController>> _mockLogger = new(MockBehavior.Loose);

        private AssetGroupController CreateSut() =>
            new(_mockLogger.Object, _mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetGroupQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetGroupDto>>
                {
                    IsSuccess = true,
                    Data = new List<AssetGroupDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllAssetGroupAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetGroupQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetGroupDto>>
                {
                    IsSuccess = true,
                    Data = new List<AssetGroupDto>()
                });

            await CreateSut().GetAllAssetGroupAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAssetGroupQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetGroupByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(AssetGroupBuilders.ValidDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetGroupAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssetGroupAutoCompleteDTO>
                {
                    AssetGroupBuilders.ValidAutoCompleteDto()
                });

            var result = await CreateSut().GetAssetGroup("AG");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateAssetGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(AssetGroupBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateAssetGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().UpdateAsync(AssetGroupBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteAssetGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().DeleteAssetGroupAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteAssetGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().DeleteAssetGroupAsync(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteAssetGroupCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
