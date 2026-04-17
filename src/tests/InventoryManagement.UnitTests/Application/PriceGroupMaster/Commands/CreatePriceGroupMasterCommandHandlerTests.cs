using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.IPriceGroupMaster;
using InventoryManagement.Application.PriceGroupMaster.Commands.CreatePriceGroupMaster;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.PriceGroupMaster.Commands
{
    public sealed class CreatePriceGroupMasterCommandHandlerTests
    {
        private readonly Mock<IPriceGroupMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IPriceGroupMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreatePriceGroupMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static CreatePriceGroupMasterCommand ValidCommand(string code = "PG001", string name = "Standard") =>
            new()
            {
                PriceGroupCode = code,
                PriceGroupName = name,
                Description = "desc",
                EffectiveFrom = DateTimeOffset.UtcNow,
                EffectiveTo = null
            };

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<InventoryManagement.Domain.Entities.PriceGroupMaster>(It.IsAny<CreatePriceGroupMasterCommand>()))
                .Returns(new InventoryManagement.Domain.Entities.PriceGroupMaster());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<InventoryManagement.Domain.Entities.PriceGroupMaster>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath(newId: 5);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(5);
            result.Message.Should().Be("Price Group created successfully.");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<InventoryManagement.Domain.Entities.PriceGroupMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(ValidCommand("PG-AUD"), CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Create" && e.ActionCode == "PRICEGROUP_CREATE"),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
