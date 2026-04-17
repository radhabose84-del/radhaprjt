using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Commands.UpdateProformaPayment;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.ProformaInvoice.Commands
{
    public sealed class UpdateProformaPaymentCommandHandlerTests
    {
        private readonly Mock<IProformaInvoiceCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IProformaInvoiceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockMisc = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateProformaPaymentCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMisc.Object, _mockMediator.Object);

        private void SetupCommon()
        {
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_FullPayment_ResolvesPaidStatus()
        {
            SetupCommon();
            _mockQueryRepo.Setup(r => r.GetProformaAmountAsync(1)).ReturnsAsync(100m);
            _mockMisc.Setup(m => m.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 5 });
            _mockCommandRepo.Setup(r => r.UpdatePaymentAsync(1, 100m, 5)).ReturnsAsync(1);

            var result = await CreateSut().Handle(
                new UpdateProformaPaymentCommand { Id = 1, PaymentReceivedAmount = 100m },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            _mockCommandRepo.Verify(r => r.UpdatePaymentAsync(1, 100m, 5), Times.Once);
        }

        [Fact]
        public async Task Handle_PartialPayment_ResolvesPartiallyPaidStatus()
        {
            SetupCommon();
            _mockQueryRepo.Setup(r => r.GetProformaAmountAsync(1)).ReturnsAsync(100m);
            _mockMisc.Setup(m => m.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 9 });
            _mockCommandRepo.Setup(r => r.UpdatePaymentAsync(1, 50m, 9)).ReturnsAsync(1);

            await CreateSut().Handle(
                new UpdateProformaPaymentCommand { Id = 1, PaymentReceivedAmount = 50m },
                CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdatePaymentAsync(1, 50m, 9), Times.Once);
        }

        [Fact]
        public async Task Handle_ZeroPayment_PassesNullStatus()
        {
            SetupCommon();
            _mockQueryRepo.Setup(r => r.GetProformaAmountAsync(1)).ReturnsAsync(100m);
            _mockCommandRepo.Setup(r => r.UpdatePaymentAsync(1, 0m, null)).ReturnsAsync(1);

            await CreateSut().Handle(
                new UpdateProformaPaymentCommand { Id = 1, PaymentReceivedAmount = 0m },
                CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdatePaymentAsync(1, 0m, null), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            SetupCommon();
            _mockQueryRepo.Setup(r => r.GetProformaAmountAsync(1)).ReturnsAsync(100m);
            _mockCommandRepo.Setup(r => r.UpdatePaymentAsync(1, 0m, null)).ReturnsAsync(1);

            await CreateSut().Handle(
                new UpdateProformaPaymentCommand { Id = 1, PaymentReceivedAmount = 0m },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "PROFORMA_PAYMENT_UPDATE"), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
