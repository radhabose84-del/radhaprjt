using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.ILogAnalysis;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.LogAnalysis.Queries.GetAllLogAnalysis;
using FinanceManagement.Application.JournalMaster.LogAnalysis.Queries.GetLogAnalysisSummary;
using MediatR;

namespace FinanceManagement.UnitTests.Application.LogAnalysis
{
    public sealed class GetAllLogAnalysisQueryHandlerTests
    {
        private readonly Mock<ILogAnalysisQueryRepository> _query = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Loose);

        [Fact]
        public async Task Handle_PassesFilters_AndReturnsPagedFeed()
        {
            _query.Setup(r => r.GetAllAsync("JournalFlag", null, null, 2, 25))
                .ReturnsAsync((new List<LogAnalysisDto> { new() { LogType = "JournalFlag", Id = 3, Summary = "Amount over limit flagged" } }, 7));

            var sut = new GetAllLogAnalysisQueryHandler(_query.Object, _mediator.Object);
            var result = await sut.Handle(new GetAllLogAnalysisQuery { PageNumber = 2, PageSize = 25, LogType = "JournalFlag" }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().ContainSingle().Which.LogType.Should().Be("JournalFlag");
            result.TotalCount.Should().Be(7);
            result.PageNumber.Should().Be(2);
            _query.Verify(r => r.GetAllAsync("JournalFlag", null, null, 2, 25), Times.Once);
        }

        [Fact]
        public async Task SummaryHandler_ReturnsCounts()
        {
            _query.Setup(r => r.GetSummaryAsync(null, null))
                .ReturnsAsync(new LogAnalysisSummaryDto
                {
                    SecurityViolationCount = 1, SequenceGapCount = 2, RecurringGenerationCount = 0, JournalFlagCount = 4, TotalCount = 7
                });

            var sut = new GetLogAnalysisSummaryQueryHandler(_query.Object);
            var result = await sut.Handle(new GetLogAnalysisSummaryQuery(), CancellationToken.None);

            result.TotalCount.Should().Be(7);
            result.JournalFlagCount.Should().Be(4);
        }
    }
}
