using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.IPriceGroupMaster;
using InventoryManagement.Application.PriceGroupMaster.Commands.UpdatePriceGroupMaster;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.PriceGroupMaster.Commands
{
    public sealed class UpdatePriceGroupMasterCommandHandlerTests
    {
        private readonly Mock<IPriceGroupMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IPriceGroupMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdatePriceGroupMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UpdatePriceGroupMasterCommand ValidCommand(int id = 1, int isActive = 1) =>
            new()
            {
                Id = id,
                PriceGroupName = "Updated",
                Description = "desc",
                EffectiveFrom = DateTimeOffset.UtcNow,
                IsActive = isActive
            };

        private void SetupHappyPath(int affectedRows = 1)
        {
            _mockMapper
                .Setup(m => m.Map<InventoryManagement.Domain.Entities.PriceGroupMaster>(It.IsAny<UpdatePriceGroupMasterCommand>()))
                .Returns(new InventoryManagement.Domain.Entities.PriceGroupMaster());
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<InventoryManagement.Domain.Entities.PriceGroupMaster>()))
                .ReturnsAsync(affectedRows);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(1);
            result.Message.Should().Be("Price Group updated successfully.");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<InventoryManagement.Domain.Entities.PriceGroupMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(ValidCommand(42), CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Update" && e.ActionCode == "PRICEGROUP_UPDATE" && e.ActionName == "42"),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
