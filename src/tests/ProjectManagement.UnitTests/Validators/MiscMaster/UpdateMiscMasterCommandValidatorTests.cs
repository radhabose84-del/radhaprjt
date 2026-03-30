using FluentValidation.TestHelper;
using ProjectManagement.Application.Common.Interfaces.IMiscMaster;
using ProjectManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using ProjectManagement.Presentation.Validation.Common;
using ProjectManagement.Presentation.Validation.MiscMaster;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Validators.MiscMaster
{
    public sealed class UpdateMiscMasterCommandValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        // MaxLengthProvider.GetMaxLength is not virtual — use a real instance with null dbContext
        // so _model is null and GetMaxLength returns null → validator uses fallback values (?? 50 / ?? 250)
        private readonly MaxLengthProvider _maxLengthProvider = new(null!);

        private UpdateMiscMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _maxLengthProvider);

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>(), id))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var command = MiscMasterBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync("MSC001", 1, 1))
                .ReturnsAsync(true);
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(1))
                .ReturnsAsync(true);
            var command = MiscMasterBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>(), 1))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(1))
                .ReturnsAsync(false);
            var command = MiscMasterBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_CodeExceedsMaxLength_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = MiscMasterBuilders.ValidUpdateCommand(code: new string('A', 51));

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }
    }
}
