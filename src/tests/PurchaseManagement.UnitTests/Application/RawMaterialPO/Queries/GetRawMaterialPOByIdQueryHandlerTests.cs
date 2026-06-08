using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Dto;
using PurchaseManagement.Application.RawMaterialPO.Queries.GetRawMaterialPOById;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.RawMaterialPO.Queries
{
    public sealed class GetRawMaterialPOByIdQueryHandlerTests
    {
        private readonly Mock<IRawMaterialPOQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMisc = new(MockBehavior.Loose);
        private readonly Mock<IRawMaterialPOFileStorage> _mockFileStorage = new(MockBehavior.Loose);

        private GetRawMaterialPOByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMisc.Object, _mockFileStorage.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(RawMaterialPOBuilders.ValidDto());

            var result = await CreateSut().Handle(new GetRawMaterialPOByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.PONumber.Should().Be("RMPO-2026-0001");
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((RawMaterialPODto?)null);

            var result = await CreateSut().Handle(new GetRawMaterialPOByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
