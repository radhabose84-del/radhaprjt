using FluentValidation.TestHelper;
using PartyManagement.Application.Common.Interfaces.IMiscMaster;
using PartyManagement.Presentation.Validation.Common;
using PartyManagement.Presentation.Validation.MiscMaster;
using PartyManagement.UnitTests.TestData;

namespace PartyManagement.UnitTests.Validators.MiscMaster
{
    public sealed class CreateMiscMasterCommandValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        // Constructor order: (IQueryRepository, MaxLengthProvider) — repo first
        private CreateMiscMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockMaxLength.Object);

        private void SetupAllAsyncMocks(string code = "MC001", int miscTypeId = 1)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(code, miscTypeId, null))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var command = MiscMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = MiscMasterBuilders.ValidCreateCommand(code: code!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
