using Contracts.Common;
using FAM.Application.AssetSubCategories.Command.CreateAssetSubCategories;
using FAM.Application.AssetSubCategories.Command.DeleteAssetSubCategories;
using FAM.Application.AssetSubCategories.Command.UpdateAssetSubCategories;
using FAM.Application.AssetSubCategories.Queries.GetAssetSubCategories;
using FAM.Application.AssetSubCategories.Queries.GetAssetSubCategoriesAutoComplete;
using FAM.Application.AssetSubCategories.Queries.GetAssetSubCategoriesById;
using FAM.Presentation.Controllers;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FixedAssetManagement.UnitTests.Controllers
{
    public sealed class AssetSubCategoriesControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<AssetSubCategoriesController>> _mockLogger = new(MockBehavior.Loose);

        private AssetSubCategoriesController CreateSut() =>
            new(_mockLogger.Object, _mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetSubCategoriesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetSubCategoriesDto>>
                {
                    IsSuccess = true,
                    Data = new List<AssetSubCategoriesDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllAssetSubCategoriesAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetSubCategoriesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetSubCategoriesDto>>
                {
                    IsSuccess = true,
                    Data = new List<AssetSubCategoriesDto>()
                });

            await CreateSut().GetAllAssetSubCategoriesAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAssetSubCategoriesQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetSubCategoriesByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(AssetSubCategoriesBuilders.ValidDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetSubCategoriesAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssetSubCategoriesAutoCompleteDto>
                {
                    AssetSubCategoriesBuilders.ValidAutoCompleteDto()
                });

            var result = await CreateSut().GetAssetSubCategories("Test");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateAssetSubCategoriesCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(AssetSubCategoriesBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateAssetSubCategoriesCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().UpdateAsync(AssetSubCategoriesBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteAssetSubCategoriesCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().DeleteAssetSubCategoriesAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteAssetSubCategoriesCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().DeleteAssetSubCategoriesAsync(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteAssetSubCategoriesCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
