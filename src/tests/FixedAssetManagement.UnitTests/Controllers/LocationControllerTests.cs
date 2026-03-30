using Contracts.Common;
using FAM.Application.Location.Command.CreateLocation;
using FAM.Application.Location.Command.DeleteLocation;
using FAM.Application.Location.Command.UpdateLocation;
using FAM.Application.Location.Queries.GetLocationAutoComplete;
using FAM.Application.Location.Queries.GetLocationById;
using FAM.Application.Location.Queries.GetLocations;
using FAM.Presentation.Controllers;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FixedAssetManagement.UnitTests.Controllers
{
    public sealed class LocationControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private LocationController CreateSut() =>
            new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetLocationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<LocationDto>>
                {
                    IsSuccess = true,
                    Data = new List<LocationDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllLocationAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetLocationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<LocationDto>>
                {
                    IsSuccess = true,
                    Data = new List<LocationDto>()
                });

            await CreateSut().GetAllLocationAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetLocationQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetLocationByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(LocationBuilders.ValidDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetLocationAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LocationAutoCompleteDto>
                {
                    new LocationAutoCompleteDto { Id = 1, LocationName = "Test Location" }
                });

            var result = await CreateSut().GetLocation("Test");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateLocationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(LocationBuilders.ValidDto());

            var result = await CreateSut().CreateAsync(LocationBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            // GetById is called first inside Update action
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetLocationByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(LocationBuilders.ValidDto());

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateLocationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Update(LocationBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteLocationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(LocationBuilders.ValidDto());

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteLocationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(LocationBuilders.ValidDto());

            await CreateSut().Delete(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteLocationCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
