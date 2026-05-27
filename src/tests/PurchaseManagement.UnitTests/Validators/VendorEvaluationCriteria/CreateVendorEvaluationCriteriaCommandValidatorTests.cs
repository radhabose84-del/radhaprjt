using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationCriteria;
using PurchaseManagement.Application.VendorEvaluationCriteria.Commands.CreateVendorEvaluationCriteria;
using PurchaseManagement.Presentation.Validation.VendorEvaluationCriteria;
using PurchaseManagement.UnitTests.TestData;
using PurchaseManagement.UnitTests.TestHelpers;

namespace PurchaseManagement.UnitTests.Validators.VendorEvaluationCriteria
{
    public sealed class CreateVendorEvaluationCriteriaCommandValidatorTests
    {
        private readonly Mock<IVendorEvaluationCriteriaQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateVendorEvaluationCriteriaCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(string code = "VEC001", int scoringMethodId = 1, int ratingImpactId = 1)
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(code, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ScoringMethodExistsAsync(scoringMethodId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.RatingImpactExistsAsync(ratingImpactId)).ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = VendorEvaluationCriteriaBuilders.ValidCreateCommand();
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = VendorEvaluationCriteriaBuilders.ValidCreateCommand(criteriaCode: code!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CriteriaCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_FailsValidation(string? name)
        {
            var command = VendorEvaluationCriteriaBuilders.ValidCreateCommand(criteriaName: name!);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CriteriaName);
        }

        [Theory]
        [InlineData("VEC-01")]
        [InlineData("VEC 01")]
        [InlineData("VEC@01")]
        public async Task Validate_NonAlphanumericCode_FailsValidation(string code)
        {
            var command = VendorEvaluationCriteriaBuilders.ValidCreateCommand(criteriaCode: code);
            SetupAllAsyncMocks(code);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CriteriaCode);
        }

        [Fact]
        public async Task Validate_CodeTooLong_FailsValidation()
        {
            var longCode = new string('A', 25);
            var command = VendorEvaluationCriteriaBuilders.ValidCreateCommand(criteriaCode: longCode);
            SetupAllAsyncMocks(longCode);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CriteriaCode);
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            var command = VendorEvaluationCriteriaBuilders.ValidCreateCommand(criteriaCode: "EXIST01");
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("EXIST01", null)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ScoringMethodExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.RatingImpactExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CriteriaCode);
        }

        [Fact]
        public async Task Validate_InvalidScoringMethodId_FailsValidation()
        {
            var command = VendorEvaluationCriteriaBuilders.ValidCreateCommand(scoringMethodId: 999);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("VEC001", null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ScoringMethodExistsAsync(999)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.RatingImpactExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ScoringMethodId);
        }

        [Fact]
        public async Task Validate_InvalidRatingImpactId_FailsValidation()
        {
            var command = VendorEvaluationCriteriaBuilders.ValidCreateCommand(ratingImpactId: 999);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("VEC001", null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ScoringMethodExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.RatingImpactExistsAsync(999)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.RatingImpactId);
        }

        [Fact]
        public async Task Validate_NegativeWeightagePercent_FailsValidation()
        {
            var command = VendorEvaluationCriteriaBuilders.ValidCreateCommand(weightagePercent: -1m);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.WeightagePercent);
        }

        [Fact]
        public async Task Validate_NegativeMinimumScore_FailsValidation()
        {
            var command = VendorEvaluationCriteriaBuilders.ValidCreateCommand(minimumScore: -5m);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MinimumScore);
        }
    }
}
