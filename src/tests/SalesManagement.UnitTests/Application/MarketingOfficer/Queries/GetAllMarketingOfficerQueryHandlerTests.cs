using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMarketingOfficer;
using SalesManagement.Application.MarketingOfficer.Dto;
using SalesManagement.Application.MarketingOfficer.Queries.GetAllMarketingOfficer;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.MarketingOfficer.Queries
{
    public class GetAllMarketingOfficerQueryHandlerTests
    {
        private readonly Mock<IMarketingOfficerQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetAllMarketingOfficerQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<List<MarketingOfficerDto>>(It.IsAny<object>()))
                .Returns<object>(o => o as List<MarketingOfficerDto> ?? new List<MarketingOfficerDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetAllMarketingOfficerQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenRepositoryReturnsData()
        {
            var query = new GetAllMarketingOfficerQuery { PageNumber = 1, PageSize = 10, SearchTerm = null };
            var dtoList = new List<MarketingOfficerDto>
            {
                MarketingOfficerBuilders.ValidDto(id: 1, employeeNo: "EMP001"),
                MarketingOfficerBuilders.ValidDto(id: 2, employeeNo: "EMP002")
            };

            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtoList, 2));

            var sut = CreateSut();

            var result = await sut.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsCorrectData_FromRepository()
        {
            var query = new GetAllMarketingOfficerQuery { PageNumber = 1, PageSize = 10, SearchTerm = null };
            var dtoList = new List<MarketingOfficerDto>
            {
                MarketingOfficerBuilders.ValidDto(id: 1, employeeNo: "EMP001"),
                MarketingOfficerBuilders.ValidDto(id: 2, employeeNo: "EMP002")
            };

            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtoList, 2));

            var sut = CreateSut();

            var result = await sut.Handle(query, CancellationToken.None);

            result.Data.Should().NotBeNull();
            result.Data!.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_ReturnsCorrectPaginationMetadata()
        {
            var query = new GetAllMarketingOfficerQuery { PageNumber = 2, PageSize = 5, SearchTerm = "EMP" };
            var dtoList = new List<MarketingOfficerDto>
            {
                MarketingOfficerBuilders.ValidDto(id: 6, employeeNo: "EMP006")
            };

            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "EMP")).ReturnsAsync((dtoList, 6));

            var sut = CreateSut();

            var result = await sut.Handle(query, CancellationToken.None);

            result.TotalCount.Should().Be(6);
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyListWithZeroCount()
        {
            var query = new GetAllMarketingOfficerQuery { PageNumber = 1, PageSize = 10, SearchTerm = "NOTFOUND" };
            var emptyList = new List<MarketingOfficerDto>();

            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, "NOTFOUND")).ReturnsAsync((emptyList, 0));

            var sut = CreateSut();

            var result = await sut.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_CallsGetAllAsync_Once_WithCorrectParameters()
        {
            var query = new GetAllMarketingOfficerQuery { PageNumber = 3, PageSize = 20, SearchTerm = "test" };
            var dtoList = new List<MarketingOfficerDto>();

            _mockQueryRepo.Setup(r => r.GetAllAsync(3, 20, "test")).ReturnsAsync((dtoList, 0));

            var sut = CreateSut();

            await sut.Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAllAsync(3, 20, "test"), Times.Once);
        }
    }
}
