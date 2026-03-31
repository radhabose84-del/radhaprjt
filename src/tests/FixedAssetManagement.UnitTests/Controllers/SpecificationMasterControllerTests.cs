using FAM.Application.SpecificationMaster.Commands.CreateSpecificationMaster;
using FAM.Application.SpecificationMaster.Commands.DeleteSpecificationMaster;
using FAM.Application.SpecificationMaster.Commands.UpdateSpecificationMaster;
using FAM.Application.SpecificationMaster.Queries.GetSpecificationMaster;
using FAM.Application.SpecificationMaster.Queries.GetSpecificationMasterAutoComplete;
using FAM.Application.SpecificationMaster.Queries.GetSpecificationMasterById;
using FAM.Presentation.Controllers;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FixedAssetManagement.UnitTests.Controllers
{
    public sealed class SpecificationMasterControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private SpecificationMasterController CreateSut() =>
            new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetSpecificationMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Contracts.Common.ApiResponseDTO<List<SpecificationMasterDTO>>
                {
                    IsSuccess = true,
                    Data = new List<SpecificationMasterDTO>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllSpecificationMasterAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetSpecificationMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Contracts.Common.ApiResponseDTO<List<SpecificationMasterDTO>>
                {
                    IsSuccess = true,
                    Data = new List<SpecificationMasterDTO>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllSpecificationMasterAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetSpecificationMasterQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetSpecificationMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SpecificationMasterBuilders.ValidDto());

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
                .Setup(m => m.Send(It.IsAny<CreateSpecificationMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SpecificationMasterBuilders.ValidDto());

            var result = await CreateSut().CreateAsync(SpecificationMasterBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateSpecificationMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SpecificationMasterBuilders.ValidDto());

            var result = await CreateSut().UpdateAsync(SpecificationMasterBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteSpecificationMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SpecificationMasterBuilders.ValidDto());

            var result = await CreateSut().DeleteAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_InvalidId_ReturnsBadRequest()
        {
            var result = await CreateSut().DeleteAsync(0);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteSpecificationMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SpecificationMasterBuilders.ValidDto());

            await CreateSut().DeleteAsync(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteSpecificationMasterCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetSpecificationMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SpecificationMasterAutoCompleteDTO>
                {
                    SpecificationMasterBuilders.ValidAutoCompleteDto()
                });

            var result = await CreateSut().GetSpecificationMaster(1, "test");

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
