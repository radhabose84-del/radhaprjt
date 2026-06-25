using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Dto;
using FinanceManagement.Application.FinancialYearMaster.Queries.GetFinancialYearMasterById;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.FinancialYearMaster.Queries
{
    public sealed class GetFinancialYearMasterByIdQueryHandlerTests
    {
        private readonly Mock<IFinancialYearMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper>   _mockMapper   = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetFinancialYearMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = FinancialYearMasterBuilders.ValidDto(id: 7);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<FinancialYearMasterDto>(It.IsAny<FinancialYearMasterDto>())).Returns(dto);

            var result = await CreateSut().Handle(new GetFinancialYearMasterByIdQuery { Id = 7 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(7);
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((FinancialYearMasterDto?)null);

            var result = await CreateSut().Handle(new GetFinancialYearMasterByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
