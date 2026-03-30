using Contracts.Common;
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.CreateAssetMasterGeneral;
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.DeleteAssetMasterGeneral;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneralById;
using FAM.Presentation.Controllers.AssetMaster;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FixedAssetManagement.UnitTests.Controllers
{
    public sealed class AssetMasterGeneralControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private AssetMasterGeneralController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetMasterGeneralQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetMasterGeneralDTO>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<AssetMasterGeneralDTO>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllAssetMasterGeneralAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetMasterGeneralQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetMasterGeneralDTO>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<AssetMasterGeneralDTO>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllAssetMasterGeneralAsync(1, 10);

            _mockMediator.Verify(m => m.Send(It.IsAny<GetAssetMasterGeneralQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetMasterGeneralByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneralById.AssetMasterDTO
                {
                    AssetName = "Test Asset"
                });

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
        public async Task Create_ReturnsStatusCode201()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateAssetMasterGeneralCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(AssetMasterGeneralBuilders.ValidAssetMasterDto());

            var result = await CreateSut().CreateAsync(AssetMasterGeneralBuilders.ValidCreateCommand());

            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteAssetMasterGeneralCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AssetMasterGeneralDTO { Id = 1 });

            var result = await CreateSut().DeleteAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
