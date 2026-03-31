using FluentValidation.TestHelper;
using PartyManagement.Application.Common.Interfaces.IMiscTypeMaster;
using PartyManagement.Presentation.Validation.Common;
using PartyManagement.Presentation.Validation.MiscTypeMaster;
using PartyManagement.UnitTests.TestData;

namespace PartyManagement.UnitTests.Validators.MiscTypeMaster
{
    public sealed class CreateMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        // Constructor order: (IQueryRepository, MaxLengthProvider) — repo first
        private CreateMiscTypeMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockMaxLength.Object);

        private void SetupAllAsyncMocks(string code = "MTY001")
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(code, null))
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

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand(code: code!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync("MTY001", null))
                .ReturnsAsync(true);
            var command = MiscTypeMasterBuilders.ValidCreateCommand(code: "MTY001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
