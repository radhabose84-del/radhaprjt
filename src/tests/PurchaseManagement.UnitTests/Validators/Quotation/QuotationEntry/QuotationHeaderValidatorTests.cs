using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationEntry;
using PurchaseManagement.Domain.Entities.Quotation.QuotationEntry;
using Application.Purchase.Quotations.Validation;

namespace PurchaseManagement.UnitTests.Validators.Quotation.QuotationEntry
{
    public sealed class QuotationHeaderValidatorTests
    {
        private readonly Mock<IQuotationCommandRepository> _mockRepo = new(MockBehavior.Loose);

        private QuotationHeaderValidator CreateValidator() =>
            new(_mockRepo.Object);

        [Fact]
        public async Task Validate_ZeroSupplierId_FailsValidation()
        {
            var header = new QuotationHeader { SupplierId = 0, RfqId = 1, QuotationNumber = "QT001" };

            var result = await CreateValidator().TestValidateAsync(header);

            result.ShouldHaveValidationErrorFor(x => x.SupplierId);
        }

        [Fact]
        public async Task Validate_EmptyQuotationNumber_FailsValidation()
        {
            var header = new QuotationHeader { SupplierId = 1, RfqId = 1, QuotationNumber = "" };

            var result = await CreateValidator().TestValidateAsync(header);

            result.ShouldHaveValidationErrorFor(x => x.QuotationNumber);
        }
    }
}
