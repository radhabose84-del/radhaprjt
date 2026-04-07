using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IPurchase.DutyMaster;
using PurchaseManagement.Application.DutyMaster.Delete;
using PurchaseManagement.Presentation.Validation.DutyMaster;

namespace PurchaseManagement.UnitTests.Validators.DutyMaster
{
    public sealed class DeleteDutyMasterValidatorTests
    {
        private readonly Mock<IDutyMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteDutyMasterValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false); // entity exists
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidationAsync(id))
                .ReturnsAsync(false); // not linked
        }

        [Fact]
        public async Task Validate_ExistingId_PassesValidation()
        {
            SetupHappyPath(1);

            var result = await CreateValidator().TestValidateAsync(new DeleteDutyMasterCommand(1));

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            // DependentRules: inner rules don't run when GreaterThan(0) fails
            var result = await CreateValidator().TestValidateAsync(new DeleteDutyMasterCommand(0));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true); // not found
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidationAsync(99))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteDutyMasterCommand(99));

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_LinkedWithOtherRecords_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false); // entity exists
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidationAsync(1))
                .ReturnsAsync(true); // linked with other records

            var result = await CreateValidator().TestValidateAsync(new DeleteDutyMasterCommand(1));

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_ZeroId_DoesNotCallNotFoundAsync()
        {
            // DependentRules ensures inner async rules don't run for invalid Id
            await CreateValidator().TestValidateAsync(new DeleteDutyMasterCommand(0));

            _mockQueryRepo.Verify(
                r => r.NotFoundAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
