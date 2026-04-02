using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries;
using InventoryManagement.Application.Item.ItemDetail.Commands.CreateItemVariants;

namespace InventoryManagement.UnitTests.Application.Item.Commands
{
    public sealed class CreateItemVariantsCommandHandlerTests
    {
        private readonly Mock<IItemCommandRepository> _mockItemRepo = new(MockBehavior.Loose);
        private readonly Mock<IItemVariantAttributeCommandRepository> _mockAttrRepo = new(MockBehavior.Loose);
        private readonly Mock<IItemVariantValueCommandRepository> _mockValueCmd = new(MockBehavior.Loose);
        private readonly Mock<IItemVariantValueQueryRepository> _mockValueQry = new(MockBehavior.Loose);
        private readonly Mock<IItemQueryRepository> _mockItemQry = new(MockBehavior.Loose);
        private readonly Mock<IItemPurchaseCommandRepository> _mockPurchaseRepo = new(MockBehavior.Loose);
        private readonly Mock<IItemInventoryCommandRepository> _mockInventoryRepo = new(MockBehavior.Loose);
        private readonly Mock<IItemQualityCommandRepository> _mockQualityRepo = new(MockBehavior.Loose);
        private readonly Mock<IItemSupplierCommandRepository> _mockSupplierRepo = new(MockBehavior.Loose);
        private readonly Mock<IItemManufactureCommandRepository> _mockManufactureRepo = new(MockBehavior.Loose);
        private readonly Mock<IItemUomCommandRepository> _mockUomRepo = new(MockBehavior.Loose);
        private readonly Mock<IUnitOfWork> _mockUow = new(MockBehavior.Loose);
        private readonly Mock<AutoMapper.IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateItemVariantsCommandHandler CreateSut() =>
            new(_mockItemRepo.Object, _mockAttrRepo.Object, _mockValueCmd.Object,
                _mockValueQry.Object, _mockItemQry.Object, _mockPurchaseRepo.Object,
                _mockInventoryRepo.Object, _mockQualityRepo.Object, _mockSupplierRepo.Object,
                _mockManufactureRepo.Object, _mockUomRepo.Object, _mockUow.Object, _mockMapper.Object);

        [Fact]
        public void CanInstantiate_CreateItemVariantsCommandHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }
    }
}
