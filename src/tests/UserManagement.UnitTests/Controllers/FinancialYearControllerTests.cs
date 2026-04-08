using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Application.FinancialYear.Command.CreateFinancialYear;
using UserManagement.Application.FinancialYear.Command.DeleteFinancialYear;
using UserManagement.Application.FinancialYear.Command.UpdateFinancialYear;
using UserManagement.Application.FinancialYear.Queries.GetFinancialYearAutoComplete;
using UserManagement.Application.FinancialYear.Queries.GetFinancialYear;
using UserManagement.Application.FinancialYear.Queries.GetFinancialYearGetById;
using UserManagement.Application.GetFinancialYearYear.Queries.GetFinancialYear;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class FinancialYearControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<FinancialYearController>> _mockLogger = new();

        private FinancialYearController CreateSut() =>
            new(_mockMediator.Object, _mockLogger.Object);

        // --- GetAllFinancialYearAsync ---

        [Fact]
        public async Task GetAllFinancialYearAsync_WithData_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetFinancialYearQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetFinancialYearDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<GetFinancialYearDto> { new GetFinancialYearDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllFinancialYearAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllFinancialYearAsync_EmptyData_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetFinancialYearQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetFinancialYearDto>>
                {
                    IsSuccess = true,
                    Message = "No records",
                    Data = new List<GetFinancialYearDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllFinancialYearAsync(1, 10);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetFinancialYearByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetFinancialYearDto> { new GetFinancialYearDto() });

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- GetAllFinancialYearAutoCompleteSearchAsync ---

        [Fact]
        public async Task GetAllFinancialYearAutoCompleteSearchAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetFinancialYearAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetFinancialYearAutoCompleteDto>());

            var result = await CreateSut().GetAllFinancialYearAutoCompleteSearchAsync("2024");

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- CreateAsync ---

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            var command = new CreateFinancialYearCommand();

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateFinancialYearCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FinancialYearDto());

            var result = await CreateSut().CreateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- UpdateAsync ---

        [Fact]
        public async Task UpdateAsync_ValidCommand_ReturnsOkResult()
        {
            var command = new UpdateFinancialYearCommand { Id = 1 };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetFinancialYearByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetFinancialYearDto> { new GetFinancialYearDto() });

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateFinancialYearCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().UpdateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_NullCommand_ReturnsBadRequest()
        {
            var result = await CreateSut().UpdateAsync(null!);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_NotFound_ReturnsNotFound()
        {
            var command = new UpdateFinancialYearCommand { Id = 99 };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetFinancialYearByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<GetFinancialYearDto>?)null);

            var result = await CreateSut().UpdateAsync(command);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        // --- Delete ---

        [Fact]
        public async Task Delete_ExistingId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetFinancialYearByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetFinancialYearDto> { new GetFinancialYearDto() });

            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteFinancialYearCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_NotFound_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetFinancialYearByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<GetFinancialYearDto>?)null);

            var result = await CreateSut().Delete(99);

            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
