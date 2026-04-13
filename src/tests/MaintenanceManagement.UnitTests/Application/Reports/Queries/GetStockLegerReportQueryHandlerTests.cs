using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IReports;
using MaintenanceManagement.Application.Reports.GetStockLegerReport;
using MaintenanceManagement.Application.StockLedger.Queries.GetStockLegerReport;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.Reports.Queries
{
    public sealed class GetStockLegerReportQueryHandlerTests
    {
        private readonly Mock<IReportRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private GetStockLegerReportQueryHandler CreateSut()
        {
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            return new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockDeptLookup.Object);
        }

        private static GetStockLegerReportQuery ValidQuery() => new()
        {
            OldUnitcode = "U01",
            FromDate = DateTime.UtcNow.AddDays(-10),
            ToDate = DateTime.UtcNow,
            ItemCode = "ITEM01",
            DepartmentId = 1
        };

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var dtos = new List<StockLedgerReportDto> { new() };
            _mockRepo.Setup(r => r.GetSubStoresStockLedger(
                It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                It.IsAny<string?>(), It.IsAny<int>())).ReturnsAsync(dtos);
            _mockMapper.Setup(m => m.Map<List<StockLedgerReportDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockRepo.Setup(r => r.GetSubStoresStockLedger(
                It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                It.IsAny<string?>(), It.IsAny<int>())).ReturnsAsync(new List<StockLedgerReportDto>());
            _mockMapper.Setup(m => m.Map<List<StockLedgerReportDto>>(It.IsAny<object>())).Returns(new List<StockLedgerReportDto>());

            var result = await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockRepo.Setup(r => r.GetSubStoresStockLedger(
                It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                It.IsAny<string?>(), It.IsAny<int>())).ReturnsAsync(new List<StockLedgerReportDto>());
            _mockMapper.Setup(m => m.Map<List<StockLedgerReportDto>>(It.IsAny<object>())).Returns(new List<StockLedgerReportDto>());

            await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            _mockRepo.Verify(r => r.GetSubStoresStockLedger(
                It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                It.IsAny<string?>(), It.IsAny<int>()), Times.Once);
        }
    }
}
