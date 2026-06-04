using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.Location.Command.CreateLocation;
using UserManagement.Application.Location.Command.DeleteLocation;
using UserManagement.Application.Location.Command.UpdateLocation;
using UserManagement.Application.Location.Queries.GetAllLocation;
using UserManagement.Application.Location.Queries.GetLocationAutoSearch;
using UserManagement.Application.Location.Queries.GetLocationById;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class LocationControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private LocationController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAllLocationAsync_WithData_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllLocationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetAllLocationDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetAllLocationDto> { new GetAllLocationDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllLocationAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllLocationAsync_EmptyData_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllLocationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetAllLocationDto>>
                {
                    IsSuccess = false,
                    Message = "No Record Found",
                    Data = new List<GetAllLocationDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllLocationAsync(1, 10);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetLocationByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LocationByIdDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateLocationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(new CreateLocationCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateLocationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().CreateAsync(new CreateLocationCommand());

            _mockMediator.Verify(m => m.Send(It.IsAny<CreateLocationCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateLocationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().UpdateAsync(new UpdateLocationCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteLocationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllLocationAutocompleteAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetLocationAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LocationAutoCompleteDto>());

            var result = await CreateSut().GetAllLocationAutocompleteAsync("test");

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
