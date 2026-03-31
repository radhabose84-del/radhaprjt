using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.Country.Commands.CreateCountry;
using UserManagement.Application.Country.Commands.DeleteCountry;
using UserManagement.Application.Country.Commands.UpdateCountry;
using UserManagement.Application.Country.Queries.GetCountries;
using UserManagement.Application.Country.Queries.GetCountryAutoComplete;
using UserManagement.Application.Country.Queries.GetCountryById;
using UserManagement.Presentation.Controllers;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Presentation.Country
{
    public sealed class CountryControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private CountryController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAllCountries_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCountryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<CountryDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<CountryDto> { CountryBuilders.ValidDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllCountriesAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllCountries_CallsMediatorSendOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCountryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<CountryDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<CountryDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllCountriesAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetCountryQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetCountryByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CountryBuilders.ValidDto());

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
            var command = CountryBuilders.ValidCreateCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateCountryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CountryBuilders.ValidDto());

            var result = await CreateSut().CreateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ValidCommand_ReturnsOkResult()
        {
            var command = CountryBuilders.ValidUpdateCommand();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateCountryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CountryBuilders.ValidDto());

            var result = await CreateSut().UpdateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_InvalidId_ReturnsBadRequest()
        {
            var command = CountryBuilders.ValidUpdateCommand(id: 0);

            var result = await CreateSut().UpdateAsync(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Delete_ValidId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteCountryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CountryBuilders.ValidDto());

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
                .Setup(m => m.Send(It.IsAny<GetCountryAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CountryBuilders.ValidAutoCompleteList());

            var result = await CreateSut().GetCountry("India");

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
