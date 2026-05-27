using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationCriteria;
using PurchaseManagement.Application.VendorEvaluationCriteria.Commands.UpdateVendorEvaluationCriteria;
using PurchaseManagement.Presentation.Validation.VendorEvaluationCriteria;
using PurchaseManagement.UnitTests.TestData;
using PurchaseManagement.UnitTests.TestHelpers;

namespace PurchaseManagement.UnitTests.Validators.VendorEvaluationCriteria
{
    public sealed class UpdateVendorEvaluationCriteriaCommandValidatorTests
    {
        private readonly Mock<IVendorEvaluationCriteriaQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateVendorEvaluationCriteriaCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1, int scoringMethodId = 1, int ratingImpactId = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ScoringMethodExistsAsync(scoringMethodId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.RatingImpactExistsAsync(ratingImpactId)).ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = VendorEvaluationCriteriaBuilders.ValidUpdateCommand();
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_FailsValidation(string? name)
        {
            var command = VendorEvaluationCriteriaBuilders.ValidUpdateCommand(criteriaName: name!);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CriteriaName);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = VendorEvaluationCriteriaBuilders.ValidUpdateCommand(id: 99);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ScoringMethodExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.RatingImpactExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_InvalidScoringMethodId_FailsValidation()
        {
            var command = VendorEvaluationCriteriaBuilders.ValidUpdateCommand(scoringMethodId: 999);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ScoringMethodExistsAsync(999)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.RatingImpactExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ScoringMethodId);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        public async Task Validate_InvalidIsActive_FailsValidation(int isActive)
        {
            var command = VendorEvaluationCriteriaBuilders.ValidUpdateCommand(isActive: isActive);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Fact]
        public async Task Validate_NegativeWeightagePercent_FailsValidation()
        {
            var command = VendorEvaluationCriteriaBuilders.ValidUpdateCommand(weightagePercent: -1m);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.WeightagePercent);
        }
    }
}
