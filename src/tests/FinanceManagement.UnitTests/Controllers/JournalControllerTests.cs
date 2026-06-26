using Microsoft.AspNetCore.Mvc;
using FinanceManagement.Presentation.Controllers.JournalMaster;
using FinanceManagement.Application.JournalMaster.Journal.Commands.CreateJournal;
using FinanceManagement.Application.JournalMaster.Journal.Commands.UpdateJournal;
using FinanceManagement.Application.JournalMaster.Journal.Commands.DeleteJournal;
using FinanceManagement.Application.JournalMaster.Journal.Commands.PostJournal;
using FinanceManagement.Application.JournalMaster.Journal.Queries.GetAllJournal;
using FinanceManagement.Application.JournalMaster.Journal.Queries.GetJournalById;
using FinanceManagement.Application.JournalMaster.Journal.Queries.GetLatePostingReport;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Controllers
{
    public sealed class JournalControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private JournalController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllJournalQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<JournalHeaderDto>> { IsSuccess = true, Data = new List<JournalHeaderDto>() });

            var result = await CreateSut().GetAllJournalAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetJournalByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(JournalBuilders.ValidDto());

            var result = await CreateSut().GetJournalByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateJournalCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreateJournal(JournalBuilders.ValidCreateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateJournalCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().UpdateJournal(JournalBuilders.ValidUpdateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Post_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<PostJournalCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<PostJournalResultDto> { IsSuccess = true, Data = JournalBuilders.ValidPostResult() });

            var result = await CreateSut().PostJournal(new PostJournalCommand(1));
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteJournalCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteJournal(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Post_CallsMediatorSendOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<PostJournalCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<PostJournalResultDto> { IsSuccess = true, Data = JournalBuilders.ValidPostResult() });

            await CreateSut().PostJournal(new PostJournalCommand(1));

            _mockMediator.Verify(m => m.Send(It.IsAny<PostJournalCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        // US-GL03-04 / AC#3 — late-posting report endpoint.

        [Fact]
        public async Task GetLatePostingReport_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetLatePostingReportQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<LatePostingReportDto>>
                {
                    IsSuccess = true,
                    Data = new List<LatePostingReportDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 50
                });

            var result = await CreateSut().GetLatePostingReportAsync();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetLatePostingReport_PropagatesFilters_ToMediator()
        {
            GetLatePostingReportQuery? captured = null;

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetLatePostingReportQuery>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<ApiResponseDTO<List<LatePostingReportDto>>>, CancellationToken>(
                    (q, _) => captured = (GetLatePostingReportQuery)q)
                .ReturnsAsync(new ApiResponseDTO<List<LatePostingReportDto>>
                {
                    IsSuccess = true,
                    Data = new List<LatePostingReportDto>(),
                    PageNumber = 2,
                    PageSize = 25
                });

            await CreateSut().GetLatePostingReportAsync(
                PageNumber: 2,
                PageSize: 25,
                AccountingPeriodId: 7,
                FromDate: new DateOnly(2026, 6, 1),
                ToDate: new DateOnly(2026, 6, 30),
                SortBy: "DaysBackdated",
                SortDirection: "DESC");

            captured.Should().NotBeNull();
            captured!.PageNumber.Should().Be(2);
            captured.PageSize.Should().Be(25);
            captured.AccountingPeriodId.Should().Be(7);
            captured.FromDate.Should().Be(new DateOnly(2026, 6, 1));
            captured.ToDate.Should().Be(new DateOnly(2026, 6, 30));
            captured.SortBy.Should().Be("DaysBackdated");
            captured.SortDirection.Should().Be("DESC");
        }
    }
}
