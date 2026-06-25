using Microsoft.AspNetCore.Mvc;
using FinanceManagement.Presentation.Controllers;
using FinanceManagement.Application.FinancialYearMaster.Commands.CreateFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Commands.UpdateFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Commands.DeleteFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Queries.GetAllFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Queries.GetFinancialYearMasterAutoComplete;
using FinanceManagement.Application.FinancialYearMaster.Queries.GetFinancialYearMasterById;
using FinanceManagement.Application.FinancialYearMaster.Queries.GetFinancialPeriodsForCompany;
using FinanceManagement.Application.FinancialYearMaster.Queries.GetPeriodForDate;
using FinanceManagement.Application.FinancialYearMaster.Dto;

namespace FinanceManagement.UnitTests.Controllers
{
    public sealed class FinancialYearMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private FinancialYearMasterController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllFinancialYearMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<FinancialYearMasterDto>>
                {
                    IsSuccess = true,
                    Data = new List<FinancialYearMasterDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllFinancialYearMasterAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllFinancialYearMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<FinancialYearMasterDto>> { Data = new() });

            await CreateSut().GetAllFinancialYearMasterAsync(1, 10);

            _mockMediator.Verify(m => m.Send(It.IsAny<GetAllFinancialYearMasterQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetFinancialYearMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FinancialYearMasterDto());

            var result = await CreateSut().GetFinancialYearMasterByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPeriodsForCompany_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetFinancialPeriodsForCompanyQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<FinancialPeriodMasterDto>)new List<FinancialPeriodMasterDto>());

            var result = await CreateSut().GetPeriodsForCompanyAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPeriodForDate_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPeriodForDateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FinancialPeriodMasterDto());

            var result = await CreateSut().GetPeriodForDateAsync(new DateOnly(2024, 4, 15));
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetFinancialYearMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<FinancialYearMasterLookupDto>)new List<FinancialYearMasterLookupDto>());

            var result = await CreateSut().GetFinancialYearMasterAutoCompleteAsync("term");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateFinancialYearMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreateFinancialYearMaster(new CreateFinancialYearMasterCommand
            {
                FinancialYearCode = "2024-25",
                StartDate = new DateOnly(2024, 4, 1),
                EndDate = new DateOnly(2025, 3, 31)
            });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateFinancialYearMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().UpdateFinancialYearMaster(new UpdateFinancialYearMasterCommand
            {
                Id = 1, FinancialYearCode = "2024-25", IsActive = 1
            });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteFinancialYearMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteFinancialYearMaster(1);
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
