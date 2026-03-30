using Contracts.Common;
using FAM.Application.Location.Command.DeleteAubLocation;
using FAM.Application.Location.Command.UpdateSubLocation;
using FAM.Application.Location.Queries.GetSubLocations;
using FAM.Application.SubLocation.Command.CreateSubLocation;
using FAM.Application.SubLocation.Queries.GetSubLocationAutoComplete;
using FAM.Application.SubLocation.Queries.GetSubLocationById;
using FAM.Application.SubLocation.Queries.GetSubLocations;
using FAM.Presentation.Controllers;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FixedAssetManagement.UnitTests.Controllers
{
    public sealed class SubLocationControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private SubLocationController CreateSut() =>
            new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetSubLocationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<SubLocationDto>>
                {
                    IsSuccess = true,
                    Data = new List<SubLocationDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllSubLocationAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetSubLocationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<SubLocationDto>>
                {
                    IsSuccess = true,
                    Data = new List<SubLocationDto>()
                });

            await CreateSut().GetAllSubLocationAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetSubLocationQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetSubLocationByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SubLocationBuilders.ValidDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetSubLocationAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SubLocationAutoCompleteDto>
                {
                    new SubLocationAutoCompleteDto { Id = 1, SubLocationName = "Test SubLocation" }
                });

            var result = await CreateSut().GetSubLocation("Test");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateSubLocationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SubLocationBuilders.ValidDto());

            var result = await CreateSut().CreateAsync(SubLocationBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            // GetById is called first inside Update action
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetSubLocationByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SubLocationBuilders.ValidDto());

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateSubLocationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Update(SubLocationBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteSubLocationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteSubLocationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Delete(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteSubLocationCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
