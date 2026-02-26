using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOffice;
using SalesManagement.Application.SalesOffice.Commands.UpdateSalesOffice;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesOffice.Commands
{
    public class UpdateSalesOfficeCommandHandlerTests
    {
        private readonly Mock<ISalesOfficeCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private UpdateSalesOfficeCommandHandler CreateSut() =>
            new UpdateSalesOfficeCommandHandler(
                _mockCommandRepo.Object,
                _mockMediator.Object,
                _mockMapper.Object);

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = SalesOfficeBuilders.ValidUpdateCommand(id: 1);
            var entity = SalesOfficeBuilders.ValidEntity(id: 1);

            _mockMapper.Setup(m => m.Map<SalesManagement.Domain.Entities.SalesOffice>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("updated");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsUpdatedId()
        {
            var command = SalesOfficeBuilders.ValidUpdateCommand(id: 5);
            var entity = SalesOfficeBuilders.ValidEntity(id: 5);

            _mockMapper.Setup(m => m.Map<SalesManagement.Domain.Entities.SalesOffice>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(5);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsync_Once()
        {
            var command = SalesOfficeBuilders.ValidUpdateCommand(id: 1);
            var entity = SalesOfficeBuilders.ValidEntity(id: 1);

            _mockMapper.Setup(m => m.Map<SalesManagement.Domain.Entities.SalesOffice>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateAsync(entity), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
        {
            var command = SalesOfficeBuilders.ValidUpdateCommand(id: 1);
            var entity = SalesOfficeBuilders.ValidEntity(id: 1);

            _mockMapper.Setup(m => m.Map<SalesManagement.Domain.Entities.SalesOffice>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.ActionCode == "SALES_OFFICE_UPDATE" &&
                        e.Module == "SalesOffice"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
