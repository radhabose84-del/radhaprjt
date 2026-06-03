using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.Station.Command.CreateStation;
using UserManagement.Application.Station.Command.DeleteStation;
using UserManagement.Application.Station.Command.UpdateStation;
using UserManagement.Application.Station.Queries.GetAllStation;
using UserManagement.Application.Station.Queries.GetStationAutoSearch;
using UserManagement.Application.Station.Queries.GetStationById;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class StationControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private StationController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAllStationAsync_WithData_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllStationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetAllStationDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetAllStationDto> { new GetAllStationDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllStationAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllStationAsync_EmptyData_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllStationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetAllStationDto>>
                {
                    IsSuccess = false,
                    Message = "No Record Found",
                    Data = new List<GetAllStationDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllStationAsync(1, 10);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetStationByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StationByIdDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateStationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(new CreateStationCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateStationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().CreateAsync(new CreateStationCommand());

            _mockMediator.Verify(m => m.Send(It.IsAny<CreateStationCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateStationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().UpdateAsync(new UpdateStationCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteStationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllStationAutocompleteAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetStationAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StationAutoCompleteDto>());

            var result = await CreateSut().GetAllStationAutocompleteAsync("test");

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
