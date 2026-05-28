using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IVendorRatingGrade;
using PurchaseManagement.Application.VendorRatingGrade.Commands.UpdateVendorRatingGrade;
using PurchaseManagement.Presentation.Validation.VendorRatingGrade;
using PurchaseManagement.UnitTests.TestData;
using PurchaseManagement.UnitTests.TestHelpers;

namespace PurchaseManagement.UnitTests.Validators.VendorRatingGrade
{
    public sealed class UpdateVendorRatingGradeCommandValidatorTests
    {
        private readonly Mock<IVendorRatingGradeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateVendorRatingGradeCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1, int? actionTypeId = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            if (actionTypeId.HasValue && actionTypeId.Value > 0)
                _mockQueryRepo.Setup(r => r.ActionTypeExistsAsync(actionTypeId.Value)).ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = VendorRatingGradeBuilders.ValidUpdateCommand();
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_FailsValidation(string? name)
        {
            var command = VendorRatingGradeBuilders.ValidUpdateCommand(gradeName: name!);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.GradeName);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = VendorRatingGradeBuilders.ValidUpdateCommand(id: 99);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ActionTypeExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_InvalidActionTypeId_FailsValidation()
        {
            var command = VendorRatingGradeBuilders.ValidUpdateCommand(actionTypeId: 999);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ActionTypeExistsAsync(999)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ActionTypeId);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        public async Task Validate_InvalidIsActive_FailsValidation(int isActive)
        {
            var command = VendorRatingGradeBuilders.ValidUpdateCommand(isActive: isActive);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Fact]
        public async Task Validate_NegativeMinScore_FailsValidation()
        {
            var command = VendorRatingGradeBuilders.ValidUpdateCommand(minScore: -1m);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MinScore);
        }
    }
}
