using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.Journal.Queries.GetLatePostingReport;

namespace FinanceManagement.UnitTests.Application.Journal.Queries
{
    /// <summary>
    /// US-GL03-04 / AC#3 — paginated late-posting report. Verifies session-company injection,
    /// repository contract, audit publishing, and pagination plumbing.
    /// </summary>
    public sealed class GetLatePostingReportQueryHandlerTests
    {
        private readonly Mock<IJournalQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private GetLatePostingReportQueryHandler CreateSut()
        {
            return new GetLatePostingReportQueryHandler(
                _mockQueryRepo.Object,
                _mockIp.Object,
                _mockMediator.Object);
        }

        private static LatePostingReportDto SampleRow(int id = 1, int daysBackdated = 5) =>
            new()
            {
                Id = id,
                CompanyId = 1,
                CompanyName = "Acme",
                VoucherTypeId = 10,
                VoucherTypeName = "JV",
                VoucherNo = $"JV/2026-27/{id:D4}",
                VoucherDate = new DateOnly(2026, 5, 1),
                AccountingPeriodId = 1,
                AccountingPeriodName = "Apr-2026",
                AccountingPeriodStatusCode = "OPEN",
                StatusId = 200,
                StatusCode = "POSTED",
                StatusName = "Posted",
                PostedAt = new DateTimeOffset(2026, 5, 1 + daysBackdated, 9, 30, 0, TimeSpan.Zero),
                PostedBy = "fmgr",
                IsBackdated = true,
                DaysBackdated = daysBackdated,
                BackdateReason = null,
                TotalDr = 5000m,
                TotalCr = 5000m,
                Narration = "Test",
                CreatedBy = 1,
                CreatedDate = DateTimeOffset.UtcNow
            };

        private void SetupHappyPath((List<LatePostingReportDto> Rows, int Total) result)
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockQueryRepo
                .Setup(r => r.GetLatePostingReportAsync(
                    It.IsAny<int>(), It.IsAny<int>(), 1,
                    It.IsAny<int?>(), It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(),
                    It.IsAny<string?>(), It.IsAny<string?>()))
                .ReturnsAsync(result);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ReturnsSuccessWithData()
        {
            var rows = new List<LatePostingReportDto> { SampleRow(), SampleRow(2, 30) };
            SetupHappyPath((rows, 2));

            var result = await CreateSut().Handle(
                new GetLatePostingReportQuery { PageNumber = 1, PageSize = 50 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Data![0].IsBackdated.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_PassesSessionCompanyId_ToRepository()
        {
            var rows = new List<LatePostingReportDto>();
            _mockIp.Setup(s => s.GetCompanyId()).Returns(42);
            _mockQueryRepo
                .Setup(r => r.GetLatePostingReportAsync(
                    1, 50, 42, null, null, null, null, null))
                .ReturnsAsync((rows, 0));
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetLatePostingReportQuery { PageNumber = 1, PageSize = 50 },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetLatePostingReportAsync(
                1, 50, 42, null, null, null, null, null), Times.Once);
        }

        [Fact]
        public async Task Handle_NoSessionCompany_Throws()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns((int?)null);

            Func<Task> act = async () => await CreateSut().Handle(
                new GetLatePostingReportQuery { PageNumber = 1, PageSize = 50 },
                CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*No active company in session*");
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            SetupHappyPath((new List<LatePostingReportDto> { SampleRow() }, 1));

            await CreateSut().Handle(
                new GetLatePostingReportQuery { PageNumber = 1, PageSize = 50 },
                CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionDetail == "GetLatePostingReportQuery"
                    && e.Module == "JournalHeader"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var rows = new List<LatePostingReportDto> { SampleRow() };
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockQueryRepo
                .Setup(r => r.GetLatePostingReportAsync(
                    2, 25, 1, 5, It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), "DaysBackdated", "DESC"))
                .ReturnsAsync((rows, 100));
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetLatePostingReportQuery
            {
                PageNumber = 2,
                PageSize = 25,
                AccountingPeriodId = 5,
                SortBy = "DaysBackdated",
                SortDirection = "DESC"
            }, CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(25);
            result.TotalCount.Should().Be(100);
        }

        [Fact]
        public async Task Handle_EmptyResult_StillReturnsSuccess()
        {
            SetupHappyPath((new List<LatePostingReportDto>(), 0));

            var result = await CreateSut().Handle(
                new GetLatePostingReportQuery { PageNumber = 1, PageSize = 50 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }
    }
}
