using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using PurchaseManagement.Presentation.Validation.PurchaseOrder.Local;

namespace PurchaseManagement.UnitTests.Validators.PurchaseOrder.Local
{
    public sealed class UpdatePurchaseOrderValidatorTests
    {
        private readonly Mock<IPurchaseOrderQueryRepository> _mockRepo = new(MockBehavior.Loose);

        private UpdatePurchaseOrderValidator CreateValidator() =>
            new(_mockRepo.Object);

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var dto = new PurchaseOrderUpdateDto { Id = 0 };

            var result = await CreateValidator().TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            _mockRepo.Setup(r => r.ExistsAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            var dto = new PurchaseOrderUpdateDto { Id = 99, VendorId = 1, CurrencyId = 1 };

            var result = await CreateValidator().TestValidateAsync(dto);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
