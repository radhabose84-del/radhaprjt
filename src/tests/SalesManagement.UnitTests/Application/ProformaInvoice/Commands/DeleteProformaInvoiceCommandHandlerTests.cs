using MediatR;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Commands.DeleteProformaInvoice;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.ProformaInvoice.Commands
{
    public sealed class DeleteProformaInvoiceCommandHandlerTests
    {
        private readonly Mock<IProformaInvoiceCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteProformaInvoiceCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new DeleteProformaInvoiceCommand(7), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_CallsSoftDeleteOnce()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(new DeleteProformaInvoiceCommand(7), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.SoftDeleteAsync(7, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(new DeleteProformaInvoiceCommand(7), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "PROFORMA_DELETE"), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
