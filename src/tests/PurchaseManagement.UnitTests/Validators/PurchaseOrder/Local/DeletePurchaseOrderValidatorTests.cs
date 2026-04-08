using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.PurchaseOrder.Local.Commands.Delete;
using PurchaseManagement.Presentation.Validation.PurchaseOrder.Local;

namespace PurchaseManagement.UnitTests.Validators.PurchaseOrder.Local
{
    public sealed class DeletePurchaseOrderValidatorTests
    {
        private readonly Mock<IPurchaseOrderQueryRepository> _mockRepo = new(MockBehavior.Strict);

        private DeletePurchaseOrderValidator CreateValidator() => new(_mockRepo.Object);

        private void SetupAllMocks(int id, bool exists = true, bool hasLinks = false)
        {
            _mockRepo.Setup(r => r.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(exists);
            _mockRepo.Setup(r => r.SoftDeleteValidationAsync(id)).ReturnsAsync(hasLinks);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllMocks(1, exists: true, hasLinks: false);

            var result = await CreateValidator().TestValidateAsync(
                new DeletePurchaseOrderCommand { Id = 1 });

            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            SetupAllMocks(99, exists: false, hasLinks: false);

            var result = await CreateValidator().TestValidateAsync(
                new DeletePurchaseOrderCommand { Id = 99 });

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_LinkedRecords_FailsValidation()
        {
            SetupAllMocks(1, exists: true, hasLinks: true);

            var result = await CreateValidator().TestValidateAsync(
                new DeletePurchaseOrderCommand { Id = 1 });

            result.ShouldHaveValidationErrorFor(x => x.Id)
                .WithErrorMessage("This master is linked with other records. You cannot delete this record.");
        }
    }
}
