using BackgroundService.Application.Common.Interfaces.IMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using BackgroundService.Presentation.Validation.Common;
using BackgroundService.Presentation.Validation.MiscTypeMaster;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.MiscTypeMaster
{
    public sealed class CreateMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateMiscTypeMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, new MaxLengthProvider(null!));

        private static CreateMiscTypeMasterCommand ValidCommand() =>
            new() { MiscTypeCode = "TESTTYPE", Description = "Test Description" };

        private void SetupAllAsyncMocks(string miscTypeCode = "TESTTYPE")
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(miscTypeCode, null))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks(command.MiscTypeCode!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyMiscTypeCode_FailsValidation(string? miscTypeCode)
        {
            var command = new CreateMiscTypeMasterCommand { MiscTypeCode = miscTypeCode, Description = "Test" };
            // AlreadyExists async rule runs even when sync rules fail
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyDescription_FailsValidation(string? description)
        {
            var command = new CreateMiscTypeMasterCommand { MiscTypeCode = "TESTTYPE", Description = description };
            SetupAllAsyncMocks("TESTTYPE");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public async Task Validate_MiscTypeCodeExceedsMaxLength_FailsValidation()
        {
            var command = new CreateMiscTypeMasterCommand
            {
                MiscTypeCode = new string('A', 51),
                Description = "Test"
            };
            SetupAllAsyncMocks(command.MiscTypeCode!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode);
        }

        [Fact]
        public async Task Validate_DescriptionExceedsMaxLength_FailsValidation()
        {
            var command = new CreateMiscTypeMasterCommand
            {
                MiscTypeCode = "TESTTYPE",
                Description = new string('A', 251)
            };
            SetupAllAsyncMocks(command.MiscTypeCode!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public async Task Validate_DuplicateMiscTypeCode_FailsValidation()
        {
            var command = ValidCommand();
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(command.MiscTypeCode, null))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode)
                .WithErrorMessage("MiscTypeCode already exists.");
        }
    }
}
