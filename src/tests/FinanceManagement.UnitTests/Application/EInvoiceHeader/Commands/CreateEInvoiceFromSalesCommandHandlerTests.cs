using Contracts.Commands.Finance;
using Contracts.Interfaces.Lookups.Sales;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Commands.CreateEInvoiceFromSales;
using Microsoft.Extensions.Logging;

namespace FinanceManagement.UnitTests.Application.EInvoiceHeader.Commands
{
    public sealed class CreateEInvoiceFromSalesCommandHandlerTests
    {
        private readonly Mock<ISalesInvoiceLookup> _mockSalesInvoiceLookup = new(MockBehavior.Loose);
        private readonly Mock<IEInvoiceHeaderCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IEInvoiceHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreateEInvoiceFromSalesCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private CreateEInvoiceFromSalesCommandHandler CreateSut() =>
            new(_mockSalesInvoiceLookup.Object, _mockCommandRepo.Object, _mockQueryRepo.Object,
                _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public void Constructor_AllDependencies_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_InvoiceNotFound_ReturnsFailure()
        {
            var result = await CreateSut().Handle(
                new CreateEInvoiceFromSalesCommand { InvoiceId = 999 },
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}
