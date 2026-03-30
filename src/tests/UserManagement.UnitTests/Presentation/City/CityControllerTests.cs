using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.City.Commands.CreateCity;
using UserManagement.Application.City.Commands.DeleteCity;
using UserManagement.Application.City.Commands.UpdateCity;
using UserManagement.Application.City.Queries.GetCities;
using UserManagement.Application.City.Queries.GetCityAutoComplete;
using UserManagement.Application.City.Queries.GetCityById;
using UserManagement.Presentation.Controllers;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Presentation.City
{
    public sealed class CityControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private CityController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAllCities_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCityQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<CityDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<CityDto> { CityBuilders.ValidDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllCitiesAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllCities_CallsMediatorSendOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCityQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<CityDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<CityDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllCitiesAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetCityQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCityByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CityBuilders.ValidDto());

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
            var command = CityBuilders.ValidCreateCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateCityCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CityBuilders.ValidDto());

            var result = await CreateSut().CreateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ValidCommand_ReturnsOkResult()
        {
            var command = CityBuilders.ValidUpdateCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateCityCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CityBuilders.ValidDto());

            var result = await CreateSut().UpdateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_InvalidStateId_ReturnsBadRequest()
        {
            var command = CityBuilders.ValidUpdateCommand(stateId: 0);

            var result = await CreateSut().UpdateAsync(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Delete_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteCityCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

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
                .Setup(m => m.Send(It.IsAny<GetCityAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CityBuilders.ValidAutoCompleteList());

            var result = await CreateSut().GetCity("Test");

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
