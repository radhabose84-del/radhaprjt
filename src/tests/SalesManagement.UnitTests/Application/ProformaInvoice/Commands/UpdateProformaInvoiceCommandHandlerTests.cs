using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Commands.UpdateProformaInvoice;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.ProformaInvoice.Commands
{
    public sealed class UpdateProformaInvoiceCommandHandlerTests
    {
        private readonly Mock<IProformaInvoiceCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IProformaInvoiceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateProformaInvoiceCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int rowsAffected = 1)
        {
            _mockMapper.Setup(m => m.Map<SalesManagement.Domain.Entities.ProformaInvoice>(It.IsAny<UpdateProformaInvoiceCommand>()))
                .Returns(new SalesManagement.Domain.Entities.ProformaInvoice());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.ProformaInvoice>()))
                .ReturnsAsync(rowsAffected);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new UpdateProformaInvoiceCommand { Id = 1, IsActive = 1 }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_CallsUpdateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(new UpdateProformaInvoiceCommand { Id = 1, IsActive = 1 }, CancellationToken.None);
            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.ProformaInvoice>()), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new UpdateProformaInvoiceCommand { Id = 1, IsActive = 1 }, CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "PROFORMA_UPDATE"), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
