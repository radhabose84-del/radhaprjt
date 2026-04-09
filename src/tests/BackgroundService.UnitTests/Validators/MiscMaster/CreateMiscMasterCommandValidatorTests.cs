using BackgroundService.Application.Interfaces.IMiscMaster;
using BackgroundService.Application.MiscMaster.Command.CreateMiscMaster;
using BackgroundService.Presentation.Validation.Common;
using BackgroundService.Presentation.Validation.MiscMaster;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.MiscMaster
{
    public sealed class CreateMiscMasterCommandValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateMiscMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, new MaxLengthProvider(null!));

        private static CreateMiscMasterCommand ValidCommand() =>
            new() { MiscTypeId = 1, Code = "TEST001", Description = "Test Description" };

        private void SetupAllAsyncMocks(string? code = "TEST001", int miscTypeId = 1)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(code!, miscTypeId, null))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks(command.Code, command.MiscTypeId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = new CreateMiscMasterCommand { MiscTypeId = 1, Code = code, Description = "Test" };
            // AlreadyExists uses composite rule on the whole command object, so setup with actual values
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>(), null))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyDescription_FailsValidation(string? description)
        {
            var command = new CreateMiscMasterCommand { MiscTypeId = 1, Code = "TEST001", Description = description };
            SetupAllAsyncMocks("TEST001", 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public async Task Validate_ZeroMiscTypeId_FailsValidation()
        {
            var command = new CreateMiscMasterCommand { MiscTypeId = 0, Code = "TEST001", Description = "Test" };
            // AlreadyExists uses composite rule - setup with It.IsAny
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>(), null))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeId);
        }

        [Fact]
        public async Task Validate_CodeExceedsMaxLength_FailsValidation()
        {
            var command = new CreateMiscMasterCommand
            {
                MiscTypeId = 1,
                Code = new string('A', 51),
                Description = "Test"
            };
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>(), null))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Fact]
        public async Task Validate_DescriptionExceedsMaxLength_FailsValidation()
        {
            var command = new CreateMiscMasterCommand
            {
                MiscTypeId = 1,
                Code = "TEST001",
                Description = new string('A', 251)
            };
            SetupAllAsyncMocks(command.Code!, command.MiscTypeId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public async Task Validate_DuplicateCodeAndMiscTypeId_FailsValidation()
        {
            var command = ValidCommand();
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(command.Code, command.MiscTypeId, null))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }
    }
}
