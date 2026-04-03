using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries;
using InventoryManagement.Application.Item.ItemAggregate.Handlers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.UnitTests.Application.Item.Commands
{
    public sealed class CreateItemCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUow = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreateItemCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IItemCommandRepository> _mockItemRepo = new(MockBehavior.Loose);
        private readonly Mock<IItemPurchaseCommandRepository> _mockPurchaseRepo = new(MockBehavior.Loose);
        private readonly Mock<IItemInventoryCommandRepository> _mockInventoryRepo = new(MockBehavior.Loose);
        private readonly Mock<IItemQualityCommandRepository> _mockQualityRepo = new(MockBehavior.Loose);
        private readonly Mock<IItemSaleCommandRepository> _mockSaleRepo = new(MockBehavior.Loose);
        private readonly Mock<IItemSupplierCommandRepository> _mockSupplierRepo = new(MockBehavior.Loose);
        private readonly Mock<IItemManufactureCommandRepository> _mockManufactureRepo = new(MockBehavior.Loose);
        private readonly Mock<IItemUomCommandRepository> _mockUomRepo = new(MockBehavior.Loose);
        private readonly Mock<IItemQueryRepository> _mockItemQry = new(MockBehavior.Loose);
        private readonly Mock<IItemVariantValueCommandRepository> _mockVariantValCmd = new(MockBehavior.Loose);
        private readonly Mock<IItemVariantValueQueryRepository> _mockVariantValQry = new(MockBehavior.Loose);
        private readonly Mock<IItemVariantAttributeCommandRepository> _mockVariantAttrCmd = new(MockBehavior.Loose);
        private readonly Mock<IItemUsageTypeMappingCommandRepository> _mockUsageTypeRepo = new(MockBehavior.Loose);

        private CreateItemCommandHandler CreateSut() =>
            new(_mockUow.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object,
                _mockItemRepo.Object, _mockPurchaseRepo.Object, _mockInventoryRepo.Object,
                _mockQualityRepo.Object, _mockSaleRepo.Object, _mockSupplierRepo.Object,
                _mockManufactureRepo.Object, _mockUomRepo.Object, _mockItemQry.Object,
                _mockVariantValCmd.Object, _mockVariantValQry.Object,
                _mockVariantAttrCmd.Object, _mockUsageTypeRepo.Object);

        [Fact]
        public void CanInstantiate_CreateItemCommandHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }
    }
}
