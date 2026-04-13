using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IReports;
using MaintenanceManagement.Application.Reports.GetCurrentAllStockItems;
using MaintenanceManagement.Application.StockLedger.Queries.GetCurrentStock;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.Reports.Queries
{
    public sealed class GetCurrentAllStockItemsQueryHandlerTests
    {
        private readonly Mock<IReportRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private GetCurrentAllStockItemsQueryHandler CreateSut()
        {
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            return new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockDeptLookup.Object);
        }

        private static GetCurrentAllStockItemsQuery ValidQuery() => new()
        {
            OldUnitcode = "U01",
            DepartmentId = 1
        };

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var dtos = new List<CurrentStockDto> { new() };
            _mockRepo.Setup(r => r.GetStockDetails(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(dtos);
            _mockMapper.Setup(m => m.Map<List<CurrentStockDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockRepo.Setup(r => r.GetStockDetails(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(new List<CurrentStockDto>());
            _mockMapper.Setup(m => m.Map<List<CurrentStockDto>>(It.IsAny<object>())).Returns(new List<CurrentStockDto>());

            var result = await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockRepo.Setup(r => r.GetStockDetails(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(new List<CurrentStockDto>());
            _mockMapper.Setup(m => m.Map<List<CurrentStockDto>>(It.IsAny<object>())).Returns(new List<CurrentStockDto>());

            await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            _mockRepo.Verify(r => r.GetStockDetails(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }
    }
}
