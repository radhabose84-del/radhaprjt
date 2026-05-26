using FluentValidation.TestHelper;
using QCManagement.Application.Common.Interfaces.IMiscTypeMaster;
using QCManagement.Presentation.Validation.MiscTypeMaster;
using QCManagement.UnitTests.TestData;
using QCManagement.UnitTests.TestHelpers;

namespace QCManagement.UnitTests.Validators.MiscTypeMaster
{
    public class UpdateMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateMiscTypeMasterCommandValidator CreateValidator()
            => new UpdateMiscTypeMasterCommandValidator(
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
            var command = MiscTypeMasterBuilders.ValidUpdateCommand();
            SetupEntityExists(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Id_ZeroOrNegative_FailsValidation(int id)
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand(id: id);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Valid Id is required.");
        }

        [Fact]
        public async Task Id_NotFound_FailsValidation()
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand(id: 999);
            SetupEntityNotFound(999);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Misc Type Master not found.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Description_Empty_FailsValidation(string? description)
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand(description: description);
            SetupEntityExists(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Description is required.");
        }

        [Fact]
        public async Task Description_TooLong_FailsValidation()
        {
            var longDesc = new string('X', 251);
            var command = MiscTypeMasterBuilders.ValidUpdateCommand(description: longDesc);
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
            var command = MiscTypeMasterBuilders.ValidUpdateCommand(isActive: isActive);
            SetupEntityExists(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive)
                  .WithErrorMessage("IsActive  must be either 0 or 1.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public async Task IsActive_ValidValue_PassesValidation(int isActive)
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand(isActive: isActive);
            SetupEntityExists(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.IsActive);
        }
    }
}
