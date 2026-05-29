using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Commands.CreateVendorEvaluationHeader;
using PurchaseManagement.Presentation.Validation.VendorEvaluationHeader;
using PurchaseManagement.UnitTests.TestData;
using PurchaseManagement.UnitTests.TestHelpers;

namespace PurchaseManagement.UnitTests.Validators.VendorEvaluationHeader
{
    public sealed class CreateVendorEvaluationHeaderCommandValidatorTests
    {
        private readonly Mock<IVendorEvaluationHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateVendorEvaluationHeaderCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int vendorId = 1, int? gradeId = 1)
        {
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.VendorExistsAsync(vendorId)).ReturnsAsync(true);
            if (gradeId.HasValue && gradeId.Value > 0)
                _mockQueryRepo.Setup(r => r.GradeExistsAsync(gradeId.Value)).ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = VendorEvaluationHeaderBuilders.ValidCreateCommand();
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_CompositeKeyDuplicate_FailsValidation()
        {
            var command = VendorEvaluationHeaderBuilders.ValidCreateCommand();
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(1, 6, 2026, null)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.VendorExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GradeExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_InvalidVendorId_FailsValidation()
        {
            var command = VendorEvaluationHeaderBuilders.ValidCreateCommand(vendorId: 999);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.VendorExistsAsync(999)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.GradeExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.VendorId);
        }

        [Fact]
        public async Task Validate_InvalidGradeId_FailsValidation()
        {
            var command = VendorEvaluationHeaderBuilders.ValidCreateCommand(gradeId: 999);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.VendorExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GradeExistsAsync(999)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.GradeId);
        }

        [Fact]
        public async Task Validate_InvalidEvaluationMonth_FailsValidation()
        {
            var command = VendorEvaluationHeaderBuilders.ValidCreateCommand(evaluationMonth: 13);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.EvaluationMonth);
        }

        [Fact]
        public async Task Validate_ZeroEvaluationMonth_FailsValidation()
        {
            var command = VendorEvaluationHeaderBuilders.ValidCreateCommand(evaluationMonth: 0);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.EvaluationMonth);
        }

        [Fact]
        public async Task Validate_NegativeTotalWeightedScore_FailsValidation()
        {
            var command = VendorEvaluationHeaderBuilders.ValidCreateCommand();
            command.TotalWeightedScore = -5m;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TotalWeightedScore);
        }

        [Fact]
        public async Task Validate_NullGradeId_PassesValidation()
        {
            var command = VendorEvaluationHeaderBuilders.ValidCreateCommand(gradeId: null);
            SetupAllAsyncMocks(gradeId: null);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
