using Contracts.Common;
using FAM.Application.AssetMaster.AssetSpecification.Commands.CreateAssetSpecification;
using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecification;
using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecificationById;
using FAM.Presentation.Controllers.AssetMaster;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FixedAssetManagement.UnitTests.Controllers
{
    public sealed class AssetSpecificationControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private AssetSpecificationController CreateSut() =>
            new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetSpecificationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetSpecificationJsonDto>>
                {
                    IsSuccess = true,
                    Data = new List<AssetSpecificationJsonDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllAssetSpecificationAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetSpecificationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetSpecificationJsonDto>>
                {
                    IsSuccess = true,
                    Data = new List<AssetSpecificationJsonDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllAssetSpecificationAsync(1, 10);

            _mockMediator.Verify(m => m.Send(It.IsAny<GetAssetSpecificationQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ValidId_ReturnsOkResult()
        {
            var jsonDto = AssetSpecificationBuilders.ValidJsonDto(1);

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetSpecificationByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(jsonDto);

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
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateAssetSpecificationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Specifications saved successfully.");

            var result = await CreateSut().CreateAsync(AssetSpecificationBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
