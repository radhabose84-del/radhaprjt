using BackgroundService.Application.Common.Interfaces.IMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using BackgroundService.Presentation.Validation.Common;
using BackgroundService.Presentation.Validation.MiscTypeMaster;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.MiscTypeMaster
{
    public sealed class UpdateMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateMiscTypeMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, new MaxLengthProvider(null!));

        private static UpdateMiscTypeMasterCommand ValidCommand(int id = 1) =>
            new()
            {
                Id = id,
                MiscTypeCode = "TESTTYPE",
                Description = "Updated Description",
                IsActive = 1
            };

        private void SetupAllAsyncMocks(string miscTypeCode = "TESTTYPE", int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(miscTypeCode, id))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(true); // true = found
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks(command.MiscTypeCode!, command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyMiscTypeCode_FailsValidation(string? miscTypeCode)
        {
            var command = new UpdateMiscTypeMasterCommand
            {
                Id = 1, MiscTypeCode = miscTypeCode, Description = "Test", IsActive = 1
            };
            // Setup all async mocks since FluentValidation runs all rules
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(1))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyDescription_FailsValidation(string? description)
        {
            var command = new UpdateMiscTypeMasterCommand
            {
                Id = 1, MiscTypeCode = "TESTTYPE", Description = description, IsActive = 1
            };
            SetupAllAsyncMocks("TESTTYPE", 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public async Task Validate_MiscTypeCodeExceedsMaxLength_FailsValidation()
        {
            var command = new UpdateMiscTypeMasterCommand
            {
                Id = 1,
                MiscTypeCode = new string('A', 51),
                Description = "Test",
                IsActive = 1
            };
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(1))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode);
        }

        [Fact]
        public async Task Validate_DuplicateMiscTypeCode_FailsValidation()
        {
            var command = ValidCommand();
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(command.MiscTypeCode, command.Id))
                .ReturnsAsync(true);

            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(command.Id))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = ValidCommand();
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(command.MiscTypeCode, command.Id))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(command.Id))
                .ReturnsAsync(false); // false = not found

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
