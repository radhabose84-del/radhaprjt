using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMarketingOfficer;
using SalesManagement.Application.MarketingOfficer.Dto;
using SalesManagement.Application.MarketingOfficer.Queries.GetMarketingOfficerById;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.MarketingOfficer.Queries
{
    public class GetMarketingOfficerByIdQueryHandlerTests
    {
        private readonly Mock<IMarketingOfficerQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetMarketingOfficerByIdQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<MarketingOfficerDto>(It.IsAny<object>()))
                .Returns<object>(o => (o as MarketingOfficerDto)!);
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetMarketingOfficerByIdQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_EntityExists_ReturnsNotNull()
        {
            var query = new GetMarketingOfficerByIdQuery { Id = 1 };
            var dto = MarketingOfficerBuilders.ValidDto(id: 1);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            var sut = CreateSut();

            var result = await sut.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsCorrectDto()
        {
            var query = new GetMarketingOfficerByIdQuery { Id = 1 };
            var dto = MarketingOfficerBuilders.ValidDto(id: 1, employeeNo: "EMP001", employeeName: "Test Officer");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            var sut = CreateSut();

            var result = await sut.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result!.EmployeeNo.Should().Be("EMP001");
            result!.EmployeeName.Should().Be("Test Officer");
        }

        [Fact]
        public async Task Handle_EntityExists_PublishesAuditEvent()
        {
            var query = new GetMarketingOfficerByIdQuery { Id = 1 };
            var dto = MarketingOfficerBuilders.ValidDto(id: 1);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            var sut = CreateSut();

            await sut.Handle(query, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_EntityExists_CallsGetByIdAsync_Once()
        {
            var query = new GetMarketingOfficerByIdQuery { Id = 7 };
            var dto = MarketingOfficerBuilders.ValidDto(id: 7);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(dto);
            var sut = CreateSut();

            await sut.Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(7), Times.Once);
        }

        [Fact]
        public async Task Handle_EntityNotFound_ReturnsNull()
        {
            var query = new GetMarketingOfficerByIdQuery { Id = 99 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((MarketingOfficerDto?)null);
            var sut = CreateSut();

            var result = await sut.Handle(query, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
