using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IMixCodeMaster;
using PurchaseManagement.Presentation.Validation.Common;
using PurchaseManagement.Presentation.Validation.MixCodeMaster;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Validators.MixCodeMaster
{
    public sealed class CreateMixCodeMasterCommandValidatorTests
    {
        private readonly Mock<IMixCodeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        // Real MaxLengthProvider with a null context → GetMaxLength returns null → validator uses fallback lengths.
        private CreateMixCodeMasterCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null!), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks()
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var command = MixCodeMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            SetupAllAsyncMocks();
            var command = MixCodeMasterBuilders.ValidCreateCommand(code: code!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MixCode);
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync("MIX001", It.IsAny<int?>()))
                .ReturnsAsync(true);
            var command = MixCodeMasterBuilders.ValidCreateCommand(code: "MIX001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MixCode);
        }

        [Theory]
        [InlineData("MIX-01")]   // hyphen
        [InlineData("MIX 01")]   // space
        [InlineData("MIX@01")]   // special char
        public async Task Validate_NonAlphanumericCode_FailsValidation(string code)
        {
            SetupAllAsyncMocks();
            var command = MixCodeMasterBuilders.ValidCreateCommand(code: code);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MixCode);
        }

        [Fact]
        public async Task Validate_EmptyDesc_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = MixCodeMasterBuilders.ValidCreateCommand(desc: "");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MixCodeDesc);
        }
    }
}
