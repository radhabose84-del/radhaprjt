using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Application.FinancialYear.Command.CreateFinancialYear;
using UserManagement.Application.FinancialYear.Command.DeleteFinancialYear;
using UserManagement.Application.FinancialYear.Command.UpdateFinancialYear;
using UserManagement.Application.FinancialYear.Queries.GetFinancialYear;
using UserManagement.Application.GetFinancialYearYear.Queries.GetFinancialYear;
using UserManagement.Application.FinancialYear.Queries.GetFinancialYearAutoComplete;
using UserManagement.Application.FinancialYear.Queries.GetFinancialYearGetById;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Presentation.FinancialYear
{
    public sealed class FinancialYearControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<FinancialYearController>> _mockLogger = new();

        private FinancialYearController CreateSut() =>
            new(_mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task GetAll_WithData_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetFinancialYearQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetFinancialYearDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetFinancialYearDto> { new() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllFinancialYearAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetFinancialYearByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetFinancialYearDto>());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetFinancialYearAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetFinancialYearAutoCompleteDto>());

            var result = await CreateSut().GetAllFinancialYearAutoCompleteSearchAsync("2024");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateFinancialYearCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FinancialYearDto());

            var result = await CreateSut().CreateAsync(new CreateFinancialYearCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetFinancialYearByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetFinancialYearDto> { new() });

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateFinancialYearCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().UpdateAsync(new UpdateFinancialYearCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetFinancialYearByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetFinancialYearDto> { new() });

            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteFinancialYearCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
