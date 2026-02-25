#nullable disable
using Contracts.Common;
using SalesManagement.Application.Common.Interfaces.ISalesOffice;
using SalesManagement.Application.SalesOffice.Dto;
using SalesManagement.Application.SalesOffice.Queries.GetSalesOfficeById;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesOffice.Queries
{
    public class GetSalesOfficeByIdQueryHandlerTests
    {
        private readonly Mock<ISalesOfficeQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetSalesOfficeByIdQueryHandler CreateSut() =>
            new GetSalesOfficeByIdQueryHandler(_mockQueryRepo.Object);

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidId_ReturnsSuccess()
        {
            var query = new GetSalesOfficeByIdQuery { Id = 1 };
            var dto = SalesOfficeBuilders.ValidDto(id: 1, name: "Office Alpha");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsCorrectDto()
        {
            var query = new GetSalesOfficeByIdQuery { Id = 1 };
            var dto = SalesOfficeBuilders.ValidDto(id: 1, name: "Office Alpha", salesOrganisationId: 2, salesOrganisationName: "Org Two");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Data.Should().NotBeNull();
            result.Data.Id.Should().Be(1);
            result.Data.SalesOfficeName.Should().Be("Office Alpha");
            result.Data.SalesOrganisationId.Should().Be(2);
            result.Data.SalesOrganisationName.Should().Be("Org Two");
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsEntityNotFoundException()
        {
            var query = new GetSalesOfficeByIdQuery { Id = 99 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((SalesOfficeDto)null);

            Func<Task> act = async () => await CreateSut().Handle(query, CancellationToken.None);

            await act.Should().ThrowAsync<EntityNotFoundException>()
                .WithMessage("*99*");
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsSuccessMessage()
        {
            var query = new GetSalesOfficeByIdQuery { Id = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(SalesOfficeBuilders.ValidDto(id: 1));

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Message.Should().Contain("retrieved");
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
