#nullable disable
using Contracts.Common;
using SalesManagement.Application.BusinessUnit.Dto;
using SalesManagement.Application.BusinessUnit.Queries.GetBusinessUnitById;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.BusinessUnit.Queries
{
    public class GetBusinessUnitByIdQueryHandlerTests
    {
        private readonly Mock<IBusinessUnitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetBusinessUnitByIdQueryHandler CreateSut() =>
            new GetBusinessUnitByIdQueryHandler(_mockQueryRepo.Object);

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_EntityExists_ReturnsSuccess()
        {
            var query = new GetBusinessUnitByIdQuery { Id = 1 };
            var dto = BusinessUnitBuilders.ValidDto(id: 1, code: "BU001");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsCorrectDto()
        {
            var query = new GetBusinessUnitByIdQuery { Id = 1 };
            var dto = BusinessUnitBuilders.ValidDto(id: 1, code: "BU001", name: "Finance Unit");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Data.Should().NotBeNull();
            result.Data.Id.Should().Be(1);
            result.Data.BusinessUnitCode.Should().Be("BU001");
            result.Data.BusinessUnitName.Should().Be("Finance Unit");
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsSuccessMessage()
        {
            var query = new GetBusinessUnitByIdQuery { Id = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(BusinessUnitBuilders.ValidDto(id: 1));

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Message.Should().Contain("retrieved");
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
        public async Task Handle_EntityNotFound_ThrowsEntityNotFoundException()
        {
            var query = new GetBusinessUnitByIdQuery { Id = 99 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((BusinessUnitDto)null);

            Func<Task> act = async () => await CreateSut().Handle(query, CancellationToken.None);

            await act.Should().ThrowAsync<EntityNotFoundException>()
                .WithMessage("*Business Unit*");
        }
    }
}
