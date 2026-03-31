using FluentValidation.TestHelper;
using PartyManagement.Application.Common.Interfaces.IMiscTypeMaster;
using PartyManagement.Presentation.Validation.Common;
using PartyManagement.Presentation.Validation.MiscTypeMaster;
using PartyManagement.UnitTests.TestData;

namespace PartyManagement.UnitTests.Validators.MiscTypeMaster
{
    public sealed class UpdateMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        // Constructor order: (IQueryRepository, MaxLengthProvider) — repo first
        private UpdateMiscTypeMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockMaxLength.Object);

        private void SetupAllAsyncMocks(string code = "MTY001", int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(code, id))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var command = MiscTypeMasterBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand(code: code!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
