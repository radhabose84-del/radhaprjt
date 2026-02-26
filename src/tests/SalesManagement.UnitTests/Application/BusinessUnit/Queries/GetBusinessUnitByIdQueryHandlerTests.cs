using AutoMapper;
using MediatR;
using SalesManagement.Application.BusinessUnit.Dto;
using SalesManagement.Application.BusinessUnit.Queries.GetBusinessUnitById;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.BusinessUnit.Queries
{
    public class GetBusinessUnitByIdQueryHandlerTests
    {
        private readonly Mock<IBusinessUnitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetBusinessUnitByIdQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<BusinessUnitDto>(It.IsAny<object>()))
                .Returns<object>(o => (o as BusinessUnitDto)!);
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetBusinessUnitByIdQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_EntityExists_ReturnsDto()
        {
            var query = new GetBusinessUnitByIdQuery { Id = 1 };
            var dto = BusinessUnitBuilders.ValidDto(id: 1, code: "BU001");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsCorrectDto()
        {
            var query = new GetBusinessUnitByIdQuery { Id = 1 };
            var dto = BusinessUnitBuilders.ValidDto(id: 1, code: "BU001", name: "Finance Unit");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result!.BusinessUnitCode.Should().Be("BU001");
            result!.BusinessUnitName.Should().Be("Finance Unit");
        }

        [Fact]
        public async Task Handle_EntityExists_CallsGetByIdAsync_Once()
        {
            var query = new GetBusinessUnitByIdQuery { Id = 7 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(BusinessUnitBuilders.ValidDto(id: 7));

            await CreateSut().Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(7), Times.Once);
        }

        [Fact]
        public async Task Handle_EntityNotFound_ReturnsNull()
        {
            var query = new GetBusinessUnitByIdQuery { Id = 99 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((BusinessUnitDto?)null);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
