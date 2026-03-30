using InventoryManagement.Application.Common.Interfaces.Item.Templates;
using InventoryManagement.Application.Item.Templates.Commands.UpdateTemplate;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Templates;
using InventoryManagement.Domain.Events;
using MediatR;
using AutoMapper;
using InspectionParameter = InventoryManagement.Domain.Entities.item.ItemDetail.Templates.InspectionParameter;

namespace InventoryManagement.UnitTests.Application.Templates.Commands
{
    public sealed class UpdateTemplateCommandHandlerTests
    {
        private readonly Mock<ITemplateCommandRepository> _mockCmd = new(MockBehavior.Strict);
        private readonly Mock<ITemplateQueryRepository> _mockQry = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateTemplateCommandHandler CreateSut() =>
            new(_mockCmd.Object, _mockQry.Object, _mockMediator.Object, _mockMapper.Object);

        private static UpdateTemplateCommand BuildCommand(int id = 1) =>
            new(id, "Updated Template", null, 1);

        [Fact]
        public async Task Handle_ExistingTemplate_ReturnsTrue()
        {
            var entity = new InspectionTemplate { Id = 1, TemplateName = "Old" };
            _mockQry.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            _mockCmd.Setup(r => r.UpdateWithParametersAsync(entity, It.IsAny<List<InspectionParameter>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(BuildCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsFalse()
        {
            _mockQry.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((InspectionTemplate?)null);

            var result = await CreateSut().Handle(BuildCommand(99), CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ExistingTemplate_PublishesAuditEvent()
        {
            var entity = new InspectionTemplate { Id = 1, TemplateName = "Old" };
            _mockQry.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            _mockCmd.Setup(r => r.UpdateWithParametersAsync(entity, It.IsAny<List<InspectionParameter>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(BuildCommand(1), CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
