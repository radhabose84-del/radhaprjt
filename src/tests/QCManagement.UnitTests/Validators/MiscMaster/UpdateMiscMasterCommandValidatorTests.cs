using FluentValidation.TestHelper;
using QCManagement.Application.Common.Interfaces.IMiscMaster;
using QCManagement.Presentation.Validation.MiscMaster;
using QCManagement.UnitTests.TestData;
using QCManagement.UnitTests.TestHelpers;

namespace QCManagement.UnitTests.Validators.MiscMaster
{
    public class UpdateMiscMasterCommandValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateMiscMasterCommandValidator CreateValidator()
            => new UpdateMiscMasterCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object);

        private void SetupEntityExists(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        }

        private void SetupEntityNotFound(int id)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);
        }

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            var command = MiscMasterBuilders.ValidUpdateCommand();
            SetupEntityExists(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Id_ZeroOrNegative_FailsValidation(int id)
        {
            var command = MiscMasterBuilders.ValidUpdateCommand(id: id);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Valid Id is required.");
        }

        [Fact]
        public async Task Id_NotFound_FailsValidation()
        {
            var command = MiscMasterBuilders.ValidUpdateCommand(id: 999);
            SetupEntityNotFound(999);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Misc Master not found.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Description_Empty_FailsValidation(string? description)
        {
            var command = MiscMasterBuilders.ValidUpdateCommand(description: description);
            SetupEntityExists(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Description is required.");
        }

        [Fact]
        public async Task Description_TooLong_FailsValidation()
        {
            var longDesc = new string('X', 251);
            var command = MiscMasterBuilders.ValidUpdateCommand(description: longDesc);
            SetupEntityExists(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Description  cannot be longer than   250 characters.");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        public async Task IsActive_InvalidValue_FailsValidation(int isActive)
        {
            var command = MiscMasterBuilders.ValidUpdateCommand(isActive: isActive);
            SetupEntityExists(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive)
                  .WithErrorMessage("IsActive  must be either 0 or 1.");
        }

        [Fact]
        public async Task SortOrder_Negative_FailsValidation()
        {
            var command = MiscMasterBuilders.ValidUpdateCommand(sortOrder: -1);
            SetupEntityExists(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SortOrder);
        }
    }
}
