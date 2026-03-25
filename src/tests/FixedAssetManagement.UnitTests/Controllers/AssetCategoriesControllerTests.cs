using Contracts.Common;
using FAM.Application.AssetCategories.Command.CreateAssetCategories;
using FAM.Application.AssetCategories.Command.DeleteAssetCategories;
using FAM.Application.AssetCategories.Command.UpdateAssetCategories;
using FAM.Application.AssetCategories.Queries.GetAssetCategories;
using FAM.Application.AssetCategories.Queries.GetAssetCategoriesAutoComplete;
using FAM.Application.AssetCategories.Queries.GetAssetCategoriesById;
using FAM.Presentation.Controllers;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FixedAssetManagement.UnitTests.Controllers
{
    public sealed class AssetCategoriesControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<AssetCategoriesController>> _mockLogger = new(MockBehavior.Loose);

        private AssetCategoriesController CreateSut() =>
            new(_mockLogger.Object, _mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetCategoriesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetCategoriesDto>>
                {
                    IsSuccess = true,
                    Data = new List<AssetCategoriesDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllAssetCategoriesAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetCategoriesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetCategoriesDto>>
                {
                    IsSuccess = true,
                    Data = new List<AssetCategoriesDto>()
                });

            await CreateSut().GetAllAssetCategoriesAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAssetCategoriesQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetCategoriesByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(AssetCategoriesBuilders.ValidDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetCategoriesAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssetCategoriesAutoCompleteDto>
                {
                    AssetCategoriesBuilders.ValidAutoCompleteDto()
                });

            var result = await CreateSut().GetAssetCategories("Test");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateAssetCategoriesCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(AssetCategoriesBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateAssetCategoriesCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().UpdateAsync(AssetCategoriesBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteAssetCategoriesCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().DeleteAssetCategoriesAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteAssetCategoriesCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().DeleteAssetCategoriesAsync(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteAssetCategoriesCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
