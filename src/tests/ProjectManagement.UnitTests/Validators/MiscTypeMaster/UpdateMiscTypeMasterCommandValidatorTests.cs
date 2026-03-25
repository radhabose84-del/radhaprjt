using FluentValidation.TestHelper;
using ProjectManagement.Application.Common.Interfaces.IMiscTypeMaster;
using ProjectManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using ProjectManagement.Presentation.Validation.Common;
using ProjectManagement.Presentation.Validation.MiscTypeMaster;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Validators.MiscTypeMaster
{
    public sealed class UpdateMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        // MaxLengthProvider.GetMaxLength is not virtual — use a real instance with null dbContext
        // so _model is null and GetMaxLength returns null → validator uses fallback values (?? 50 / ?? 250)
        private readonly MaxLengthProvider _maxLengthProvider = new(null!);

        private UpdateMiscTypeMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _maxLengthProvider);

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(It.IsAny<int>()))
                .ReturnsAsync(true); // true = entity was found (repo method inverted naming)
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand();
            SetupAllAsyncMocks(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand(miscTypeCode: code);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand(id: 999);

            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(999))
                .ReturnsAsync(false); // false = entity not found

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
