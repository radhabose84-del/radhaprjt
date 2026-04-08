using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationEntry;
using PurchaseManagement.Application.Quotations.QuotationEntry.Commands.Update;
using PurchaseManagement.Application.Quotations.QuotationEntry.DTOs;
using PurchaseManagement.Domain.Entities.Quotation.QuotationEntry;

namespace PurchaseManagement.UnitTests.Application.QuotationEntry.Commands
{
    public sealed class UpdateQuotationCommandHandlerTests
    {
        private readonly Mock<IQuotationCommandRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<ILogger<UpdateQuotationHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);

        private UpdateQuotationHandler CreateSut() =>
            new(_mockRepo.Object, _mockLogger.Object, _mockIp.Object,
                _mockUnitLookup.Object, _mockCompanyLookup.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsUnit()
        {
            var existing = new QuotationHeader
            {
                Id = 1,
                SupplierId = 1,
                RfqId = 1,
                Lines = new List<QuotationDetail>()
            };

            _mockRepo
                .Setup(r => r.GetWithLinesAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            _mockRepo
                .Setup(r => r.ExistsForSupplierRfqOtherAsync(1, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockRepo
                .Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockIp.Setup(i => i.GetUnitId()).Returns(1);

            var command = new UpdateQuotationCommand(
                Id: 1, SupplierId: 1, RfqId: 1, QuotationNumber: "Q001",
                ValidTill: DateOnly.FromDateTime(DateTime.Today),
                FreightModeId: 1, Freight: 100m, PaymentTermsId: 1,
                IncotermsId: 1, InsuranceCharge: 50m,
                TaxableSubtotal: 1000m, TaxableGst: 180m,
                TaxableTotal: 1180m, GrandTotal: 1280m,
                QuotationImage: "", Lines: new List<QuotationDetailDto>(),
                IsActive: 1
            );

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(Unit.Value);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsEntityNotFoundException()
        {
            _mockRepo
                .Setup(r => r.GetWithLinesAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((QuotationHeader?)null);

            var command = new UpdateQuotationCommand(
                Id: 99, SupplierId: 1, RfqId: 1, QuotationNumber: "Q001",
                ValidTill: DateOnly.FromDateTime(DateTime.Today),
                FreightModeId: 1, Freight: 100m, PaymentTermsId: 1,
                IncotermsId: 1, InsuranceCharge: 50m,
                TaxableSubtotal: 1000m, TaxableGst: 180m,
                TaxableTotal: 1180m, GrandTotal: 1280m,
                QuotationImage: "", Lines: new List<QuotationDetailDto>(),
                IsActive: 1
            );

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }
    }
}
