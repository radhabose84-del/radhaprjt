using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationEntry;
using PurchaseManagement.Application.Quotations.QuotationEntry.Commands.Create;
using PurchaseManagement.Application.Quotations.QuotationEntry.DTOs;

namespace PurchaseManagement.UnitTests.Application.QuotationEntry.Commands
{
    public sealed class CreateQuotationCommandHandlerTests
    {
        private readonly Mock<IQuotationCommandRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreateQuotationHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);

        private CreateQuotationHandler CreateSut() =>
            new(_mockRepo.Object, _mockLogger.Object, _mockIp.Object,
                _mockUnitLookup.Object, _mockCompanyLookup.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            _mockRepo
                .Setup(r => r.ExistsForSupplierRfqAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockRepo
                .Setup(r => r.AddAsync(It.IsAny<PurchaseManagement.Domain.Entities.Quotation.QuotationEntry.QuotationHeader>(), It.IsAny<CancellationToken>()))
                .Callback<PurchaseManagement.Domain.Entities.Quotation.QuotationEntry.QuotationHeader, CancellationToken>((h, _) => h.Id = 1)
                .Returns(Task.CompletedTask);

            _mockRepo
                .Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockIp.Setup(i => i.GetUnitId()).Returns(1);

            var command = new CreateQuotationCommand(
                RfqId: 1,
                SupplierId: 1,
                QuotationNumber: "Q001",
                ValidTill: DateOnly.FromDateTime(DateTime.Today),
                FreightModeId: 1,
                Freight: 100m,
                PaymentTermsId: 1,
                IncotermsId: 1,
                InsuranceCharge: 50m,
                QuotationImage: null,
                TaxableSubtotal: 1000m,
                TaxableGst: 180m,
                TaxableTotal: 1180m,
                GrandTotal: 1280m,
                Lines: new List<QuotationDetailDto>()
            );

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_DuplicateSupplierRfq_ThrowsInvalidOperation()
        {
            _mockRepo
                .Setup(r => r.ExistsForSupplierRfqAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var command = new CreateQuotationCommand(
                RfqId: 1,
                SupplierId: 1,
                QuotationNumber: "Q001",
                ValidTill: DateOnly.FromDateTime(DateTime.Today),
                FreightModeId: 1,
                Freight: 100m,
                PaymentTermsId: 1,
                IncotermsId: 1,
                InsuranceCharge: 50m,
                QuotationImage: null,
                TaxableSubtotal: 1000m,
                TaxableGst: 180m,
                TaxableTotal: 1180m,
                GrandTotal: 1280m,
                Lines: new List<QuotationDetailDto>()
            );

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsSaveOnce()
        {
            _mockRepo
                .Setup(r => r.ExistsForSupplierRfqAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockRepo
                .Setup(r => r.AddAsync(It.IsAny<PurchaseManagement.Domain.Entities.Quotation.QuotationEntry.QuotationHeader>(), It.IsAny<CancellationToken>()))
                .Callback<PurchaseManagement.Domain.Entities.Quotation.QuotationEntry.QuotationHeader, CancellationToken>((h, _) => h.Id = 1)
                .Returns(Task.CompletedTask);

            _mockRepo
                .Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockIp.Setup(i => i.GetUnitId()).Returns(1);

            var command = new CreateQuotationCommand(
                RfqId: 1, SupplierId: 1, QuotationNumber: "Q001",
                ValidTill: DateOnly.FromDateTime(DateTime.Today),
                FreightModeId: 1, Freight: 100m, PaymentTermsId: 1,
                IncotermsId: 1, InsuranceCharge: 50m, QuotationImage: null,
                TaxableSubtotal: 1000m, TaxableGst: 180m, TaxableTotal: 1180m,
                GrandTotal: 1280m, Lines: new List<QuotationDetailDto>()
            );

            await CreateSut().Handle(command, CancellationToken.None);

            _mockRepo.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
