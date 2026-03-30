using InventoryManagement.Application.Common.Interfaces.Item.Templates;
using InventoryManagement.Application.Item.Templates.Commands.CreateTemplate;
using InventoryManagement.Domain.Events;
using MediatR;
using AutoMapper;

namespace InventoryManagement.UnitTests.Application.Templates.Commands
{
    public sealed class CreateTemplateCommandHandlerTests
    {
        private readonly Mock<ITemplateCommandRepository> _mockCmd = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateTemplateCommandHandler CreateSut() =>
            new(_mockCmd.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            _mockCmd.Setup(r => r.CreateAsync(It.IsAny<InventoryManagement.Domain.Entities.Item.ItemDetail.Templates.InspectionTemplate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(7);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new CreateTemplateCommand("TestTemplate", null), CancellationToken.None);

            result.Should().Be(7);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            _mockCmd.Setup(r => r.CreateAsync(It.IsAny<InventoryManagement.Domain.Entities.Item.ItemDetail.Templates.InspectionTemplate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new CreateTemplateCommand("T1", null), CancellationToken.None);

            _mockCmd.Verify(r => r.CreateAsync(It.IsAny<InventoryManagement.Domain.Entities.Item.ItemDetail.Templates.InspectionTemplate>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            _mockCmd.Setup(r => r.CreateAsync(It.IsAny<InventoryManagement.Domain.Entities.Item.ItemDetail.Templates.InspectionTemplate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new CreateTemplateCommand("T1", null), CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
