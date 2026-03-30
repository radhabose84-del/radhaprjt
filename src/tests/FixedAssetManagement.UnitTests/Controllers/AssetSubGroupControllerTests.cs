using Contracts.Common;
using FAM.Application.AssetSubGroup.Command.CreateAssetSubGroup;
using FAM.Application.AssetSubGroup.Command.DeleteAssetSubGroup;
using FAM.Application.AssetSubGroup.Command.UpdateAssetSubGroup;
using FAM.Application.AssetSubGroup.Queries.GetAssetSubGroup;
using FAM.Application.AssetSubGroup.Queries.GetAssetSubGroupAutoComplete;
using FAM.Application.AssetSubGroup.Queries.GetAssetSubGroupById;
using FAM.Presentation.Controllers;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FixedAssetManagement.UnitTests.Controllers
{
    public sealed class AssetSubGroupControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<AssetSubGroupController>> _mockLogger = new(MockBehavior.Loose);

        private AssetSubGroupController CreateSut() =>
            new(_mockLogger.Object, _mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetSubGroupQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetSubGroupDto>>
                {
                    IsSuccess = true,
                    Data = new List<AssetSubGroupDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllAssetSubGroupAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetSubGroupQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetSubGroupDto>>
                {
                    IsSuccess = true,
                    Data = new List<AssetSubGroupDto>()
                });

            await CreateSut().GetAllAssetSubGroupAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAssetSubGroupQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetSubGroupByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(AssetSubGroupBuilders.ValidDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetSubGroupAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssetSubGroupAutoCompleteDTO>
                {
                    AssetSubGroupBuilders.ValidAutoCompleteDto()
                });

            var result = await CreateSut().GetAssetSubGroup("SG");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateAssetSubGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(AssetSubGroupBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateAssetSubGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().UpdateAsync(AssetSubGroupBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteAssetSubGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().DeleteAssetSubGroupAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteAssetSubGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().DeleteAssetSubGroupAsync(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteAssetSubGroupCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
