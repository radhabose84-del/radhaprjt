using Contracts.Common;
using FAM.Application.Manufacture.Commands.CreateManufacture;
using FAM.Application.Manufacture.Commands.DeleteManufacture;
using FAM.Application.Manufacture.Commands.UpdateManufacture;
using FAM.Application.Manufacture.Queries.GetManufacture;
using FAM.Application.Manufacture.Queries.GetManufactureAutoComplete;
using FAM.Application.Manufacture.Queries.GetManufactureById;
using FAM.Presentation.Controllers;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FixedAssetManagement.UnitTests.Controllers
{
    public sealed class ManufactureControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private ManufactureController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetManufactureQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<ManufactureDTO>>
                {
                    IsSuccess = true,
                    Data = new List<ManufactureDTO>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllCitiesAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetManufactureByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ManufacturesBuilders.ValidDto());

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
                .Setup(m => m.Send(It.IsAny<CreateManufactureCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ManufacturesBuilders.ValidDto());

            var result = await CreateSut().CreateAsync(ManufacturesBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateManufactureCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateAsync(ManufacturesBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteManufactureCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ManufacturesBuilders.ValidDto());

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
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetManufactureAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ManufactureAutoCompleteDTO> { ManufacturesBuilders.ValidAutoCompleteDto() });

            var result = await CreateSut().GetManufacture("mfg");

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
