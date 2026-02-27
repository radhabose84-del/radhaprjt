using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOffice;
using SalesManagement.Application.SalesOffice.Dto;
using SalesManagement.Application.SalesOffice.Queries.GetSalesOfficeById;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesOffice.Queries
{
    public class GetSalesOfficeByIdQueryHandlerTests
    {
        private readonly Mock<ISalesOfficeQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetSalesOfficeByIdQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<SalesOfficeDto>(It.IsAny<object>()))
                .Returns<object>(o => (o as SalesOfficeDto)!);
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetSalesOfficeByIdQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidId_ReturnsNotNull()
        {
            var query = new GetSalesOfficeByIdQuery { Id = 1 };
            var dto = SalesOfficeBuilders.ValidDto(id: 1, name: "Office Alpha");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsCorrectDto()
        {
            var query = new GetSalesOfficeByIdQuery { Id = 1 };
            var dto = SalesOfficeBuilders.ValidDto(id: 1, name: "Office Alpha", salesOrganisationId: 2, salesOrganisationName: "Org Two");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result!.SalesOfficeName.Should().Be("Office Alpha");
            result!.SalesOrganisationId.Should().Be(2);
            result!.SalesOrganisationName.Should().Be("Org Two");
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            var query = new GetSalesOfficeByIdQuery { Id = 99 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((SalesOfficeDto?)null);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            var query = new GetSalesOfficeByIdQuery { Id = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(SalesOfficeBuilders.ValidDto(id: 1));

            await CreateSut().Handle(query, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_CallsGetByIdAsync_Once()
        {
            var query = new GetSalesOfficeByIdQuery { Id = 7 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(SalesOfficeBuilders.ValidDto(id: 7));

            await CreateSut().Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(7), Times.Once);
        }
    }
}
