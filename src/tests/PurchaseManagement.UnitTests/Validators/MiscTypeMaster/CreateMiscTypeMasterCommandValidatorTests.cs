using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IMiscTypeMaster;
using PurchaseManagement.Presentation.Validation.Common;
using PurchaseManagement.Presentation.Validation.MiscTypeMaster;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Validators.MiscTypeMaster
{
    public sealed class CreateMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateMiscTypeMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockMaxLength.Object);

        private void SetupAllAsyncMocks()
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var command = MiscTypeMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null))
                .ReturnsAsync(true);
            var command = MiscTypeMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData("QA MISC")]   // space        — not alphanumeric (pattern ^[A-Za-z0-9]+$)
        [InlineData("QA@MISC")]   // special char — not alphanumeric
        [InlineData("TYP-01")]    // hyphen       — not alphanumeric
        public async Task Validate_NonAlphanumericCode_FailsValidation(string code)
        {
            SetupAllAsyncMocks();
            var command = MiscTypeMasterBuilders.ValidCreateCommand(code: code);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode);
        }
    }
}
