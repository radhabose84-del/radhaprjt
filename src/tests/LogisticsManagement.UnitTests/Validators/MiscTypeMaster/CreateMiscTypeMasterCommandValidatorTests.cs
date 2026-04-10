using FluentValidation.TestHelper;
using LogisticsManagement.Application.Common.Interfaces.IMiscTypeMaster;
using LogisticsManagement.Application.MiscTypeMaster.Commands.CreateMiscTypeMaster;
using LogisticsManagement.Presentation.Validation.Common;
using LogisticsManagement.Presentation.Validation.MiscTypeMaster;
using LogisticsManagement.UnitTests.TestData;

namespace LogisticsManagement.UnitTests.Validators.MiscTypeMaster
{
    public sealed class CreateMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateMiscTypeMasterCommandValidator CreateValidator()
        {
            var maxLengthProvider = new MaxLengthProvider(null!);
            return new CreateMiscTypeMasterCommandValidator(maxLengthProvider, _mockQueryRepo.Object);
        }

        private void SetupAllAsyncMocks(string miscTypeCode = "FREIGHT")
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(miscTypeCode, null)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand();
            SetupAllAsyncMocks(command.MiscTypeCode!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyMiscTypeCode_FailsValidation(string? code)
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand(miscTypeCode: code);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyDescription_FailsValidation(string? description)
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand(description: description);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Theory]
        [InlineData("CODE-01")]
        [InlineData("CODE 01")]
        [InlineData("CODE@01")]
        public async Task Validate_NonAlphanumericCode_FailsValidation(string code)
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand(miscTypeCode: code);
            SetupAllAsyncMocks(code);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode);
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand(miscTypeCode: "EXIST001");
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("EXIST001", null)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode);
        }

        [Fact]
        public async Task Validate_AlphanumericCode_PassesValidation()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand(miscTypeCode: "FREIGHT01");
            SetupAllAsyncMocks("FREIGHT01");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.MiscTypeCode);
        }
    }
}
