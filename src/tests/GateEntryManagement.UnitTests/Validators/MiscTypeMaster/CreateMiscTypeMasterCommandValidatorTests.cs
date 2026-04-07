using FluentValidation.TestHelper;
using GateEntryManagement.Application.Common.Interfaces.IMiscTypeMaster;
using GateEntryManagement.Application.MiscTypeMaster.Commands.CreateMiscTypeMaster;
using GateEntryManagement.Presentation.Validation.Common;
using GateEntryManagement.Presentation.Validation.MiscTypeMaster;

namespace GateEntryManagement.UnitTests.Validators.MiscTypeMaster
{
    public sealed class CreateMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateMiscTypeMasterCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(string code = "MTYPE001")
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(code, null)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new CreateMiscTypeMasterCommand
            {
                MiscTypeCode = "MTYPE001",
                Description = "Test Misc Type"
            };
            SetupAllAsyncMocks(command.MiscTypeCode!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = new CreateMiscTypeMasterCommand
            {
                MiscTypeCode = code,
                Description = "Test Misc Type"
            };
            // No async mocks needed - code is null/empty so AlreadyExists .When() guard skips

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode);
        }

        [Fact]
        public async Task Validate_EmptyDescription_FailsValidation()
        {
            var command = new CreateMiscTypeMasterCommand
            {
                MiscTypeCode = "MTYPE001",
                Description = ""
            };
            SetupAllAsyncMocks(command.MiscTypeCode!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            var command = new CreateMiscTypeMasterCommand
            {
                MiscTypeCode = "EXIST001",
                Description = "Test Misc Type"
            };
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("EXIST001", null)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode);
        }

        [Theory]
        [InlineData("CODE-01")]
        [InlineData("CODE 01")]
        [InlineData("CODE@01")]
        public async Task Validate_NonAlphanumericCode_FailsValidation(string code)
        {
            var command = new CreateMiscTypeMasterCommand
            {
                MiscTypeCode = code,
                Description = "Test Misc Type"
            };
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(code, null)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode);
        }
    }
}
