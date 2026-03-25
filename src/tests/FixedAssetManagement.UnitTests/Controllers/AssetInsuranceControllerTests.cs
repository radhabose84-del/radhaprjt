using Contracts.Common;
using FAM.Application.AssetMaster.AssetInsurance.Commands.CreateAssetInsurance;
using FAM.Application.AssetMaster.AssetInsurance.Commands.DeleteAssetInsurance;
using FAM.Application.AssetMaster.AssetInsurance.Commands.UpdateAssetInsurance;
using FAM.Application.AssetMaster.AssetInsurance.Queries.GetAssetInsurance;
using FAM.Application.AssetMaster.AssetInsurance.Queries.GetAssetInsuranceById;
using FAM.Presentation.Controllers.AssetMaster;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FixedAssetManagement.UnitTests.Controllers
{
    public sealed class AssetInsuranceControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private AssetInsuranceController CreateSut() =>
            new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetInsuranceQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetAssetInsuranceDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetAssetInsuranceDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllAssetInsuranceAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetInsuranceQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetAssetInsuranceDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetAssetInsuranceDto>()
                });

            await CreateSut().GetAllAssetInsuranceAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAssetInsuranceQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetInsuranceByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(AssetInsuranceBuilders.ValidDto());

            var result = await CreateSut().GetByAssetIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsStatusCode201()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateAssetInsuranceCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(AssetInsuranceBuilders.ValidDto());

            var result = await CreateSut().CreateAsync(AssetInsuranceBuilders.ValidCreateCommand());

            result.Should().BeOfType<ObjectResult>();
        }

        [Fact]
        public async Task Create_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateAssetInsuranceCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(AssetInsuranceBuilders.ValidDto());

            await CreateSut().CreateAsync(AssetInsuranceBuilders.ValidCreateCommand());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateAssetInsuranceCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateAssetInsuranceCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Update(AssetInsuranceBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_WithValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteAssetInsuranceCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteAssetInsuranceCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Delete(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteAssetInsuranceCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
