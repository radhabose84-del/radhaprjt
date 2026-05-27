using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IVendorRatingGrade;
using PurchaseManagement.Application.VendorRatingGrade.Commands.CreateVendorRatingGrade;
using PurchaseManagement.Presentation.Validation.VendorRatingGrade;
using PurchaseManagement.UnitTests.TestData;
using PurchaseManagement.UnitTests.TestHelpers;

namespace PurchaseManagement.UnitTests.Validators.VendorRatingGrade
{
    public sealed class CreateVendorRatingGradeCommandValidatorTests
    {
        private readonly Mock<IVendorRatingGradeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateVendorRatingGradeCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(string code = "GRD001", int? actionTypeId = 1)
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(code, null)).ReturnsAsync(false);
            if (actionTypeId.HasValue && actionTypeId.Value > 0)
                _mockQueryRepo.Setup(r => r.ActionTypeExistsAsync(actionTypeId.Value)).ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = VendorRatingGradeBuilders.ValidCreateCommand();
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = VendorRatingGradeBuilders.ValidCreateCommand(gradeCode: code!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.GradeCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_FailsValidation(string? name)
        {
            var command = VendorRatingGradeBuilders.ValidCreateCommand(gradeName: name!);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.GradeName);
        }

        [Theory]
        [InlineData("GRD-01")]
        [InlineData("GRD 01")]
        [InlineData("GRD@01")]
        public async Task Validate_NonAlphanumericCode_FailsValidation(string code)
        {
            var command = VendorRatingGradeBuilders.ValidCreateCommand(gradeCode: code);
            SetupAllAsyncMocks(code);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.GradeCode);
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            var command = VendorRatingGradeBuilders.ValidCreateCommand(gradeCode: "EXIST01");
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("EXIST01", null)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ActionTypeExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.GradeCode);
        }

        [Fact]
        public async Task Validate_InvalidActionTypeId_FailsValidation()
        {
            var command = VendorRatingGradeBuilders.ValidCreateCommand(actionTypeId: 999);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("GRD001", null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ActionTypeExistsAsync(999)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ActionTypeId);
        }

        [Fact]
        public async Task Validate_NullActionTypeId_PassesValidation()
        {
            var command = VendorRatingGradeBuilders.ValidCreateCommand(actionTypeId: null);
            SetupAllAsyncMocks("GRD001", null);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NegativeMinScore_FailsValidation()
        {
            var command = VendorRatingGradeBuilders.ValidCreateCommand(minScore: -1m);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MinScore);
        }

        [Fact]
        public async Task Validate_NegativeMaxScore_FailsValidation()
        {
            var command = VendorRatingGradeBuilders.ValidCreateCommand(maxScore: -1m);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MaxScore);
        }
    }
}
