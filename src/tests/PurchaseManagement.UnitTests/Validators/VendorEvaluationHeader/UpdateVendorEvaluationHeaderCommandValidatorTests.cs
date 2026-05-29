using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Commands.UpdateVendorEvaluationHeader;
using PurchaseManagement.Presentation.Validation.VendorEvaluationHeader;
using PurchaseManagement.UnitTests.TestData;
using PurchaseManagement.UnitTests.TestHelpers;

namespace PurchaseManagement.UnitTests.Validators.VendorEvaluationHeader
{
    public sealed class UpdateVendorEvaluationHeaderCommandValidatorTests
    {
        private readonly Mock<IVendorEvaluationHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateVendorEvaluationHeaderCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1, int vendorId = 1, int? gradeId = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.VendorExistsAsync(vendorId)).ReturnsAsync(true);
            if (gradeId.HasValue && gradeId.Value > 0)
                _mockQueryRepo.Setup(r => r.GradeExistsAsync(gradeId.Value)).ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = VendorEvaluationHeaderBuilders.ValidUpdateCommand();
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = VendorEvaluationHeaderBuilders.ValidUpdateCommand(id: 99);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.VendorExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GradeExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_CompositeKeyDuplicate_FailsValidation()
        {
            var command = VendorEvaluationHeaderBuilders.ValidUpdateCommand();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(1, 6, 2026, 1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.VendorExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GradeExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_InvalidVendorId_FailsValidation()
        {
            var command = VendorEvaluationHeaderBuilders.ValidUpdateCommand(vendorId: 999);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.VendorExistsAsync(999)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.GradeExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.VendorId);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        public async Task Validate_InvalidIsActive_FailsValidation(int isActive)
        {
            var command = VendorEvaluationHeaderBuilders.ValidUpdateCommand(isActive: isActive);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Fact]
        public async Task Validate_NegativeTotalWeightedScore_FailsValidation()
        {
            var command = VendorEvaluationHeaderBuilders.ValidUpdateCommand();
            command.TotalWeightedScore = -5m;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TotalWeightedScore);
        }

        [Fact]
        public async Task Validate_InvalidEvaluationMonth_FailsValidation()
        {
            var command = VendorEvaluationHeaderBuilders.ValidUpdateCommand(evaluationMonth: 0);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.EvaluationMonth);
        }
    }
}
