using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Application.VoucherType.Dto;
using FinanceManagement.Application.VoucherType.Queries.GetVoucherTypeById;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.VoucherType.Queries
{
    public sealed class GetVoucherTypeByIdQueryHandlerTests
    {
        private readonly Mock<IVoucherTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetVoucherTypeByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = VoucherTypeBuilders.ValidDto(id: 5);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<VoucherTypeMasterDto>(It.IsAny<object>())).Returns(dto);

            var result = await CreateSut().Handle(new GetVoucherTypeByIdQuery { Id = 5 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(5);
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((VoucherTypeMasterDto?)null);

            var result = await CreateSut().Handle(new GetVoucherTypeByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
