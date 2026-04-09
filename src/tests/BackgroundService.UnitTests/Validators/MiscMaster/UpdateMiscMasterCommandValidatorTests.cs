using BackgroundService.Application.Interfaces.IMiscMaster;
using BackgroundService.Application.MiscMaster.Command.UpdateMiscMaster;
using BackgroundService.Presentation.Validation.Common;
using BackgroundService.Presentation.Validation.MiscMaster;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.MiscMaster
{
    public sealed class UpdateMiscMasterCommandValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateMiscMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, new MaxLengthProvider(null!));

        private static UpdateMiscMasterCommand ValidCommand(int id = 1) =>
            new()
            {
                Id = id,
                MiscTypeId = 1,
                Code = "TEST001",
                Description = "Updated Description",
                SortOrder = 1,
                IsActive = 1
            };

        private void SetupAllAsyncMocks(string? code = "TEST001", int miscTypeId = 1, int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(code!, miscTypeId, id))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks(command.Code!, command.MiscTypeId, command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = new UpdateMiscMasterCommand
            {
                Id = 1, MiscTypeId = 1, Code = code, Description = "Test", IsActive = 1
            };
            // Setup all async mocks since FluentValidation runs all rules
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(1))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyDescription_FailsValidation(string? description)
        {
            var command = new UpdateMiscMasterCommand
            {
                Id = 1, MiscTypeId = 1, Code = "TEST001", Description = description, IsActive = 1
            };
            SetupAllAsyncMocks("TEST001", 1, 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public async Task Validate_ZeroMiscTypeId_FailsValidation()
        {
            var command = new UpdateMiscMasterCommand
            {
                Id = 1, MiscTypeId = 0, Code = "TEST001", Description = "Test", IsActive = 1
            };
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(1))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeId);
        }

        [Fact]
        public async Task Validate_CodeExceedsMaxLength_FailsValidation()
        {
            var command = new UpdateMiscMasterCommand
            {
                Id = 1, MiscTypeId = 1, Code = new string('A', 51), Description = "Test", IsActive = 1
            };
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(1))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            var command = ValidCommand();
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(command.Code, command.MiscTypeId, command.Id))
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
                .Setup(r => r.AlreadyExistsAsync(command.Code, command.MiscTypeId, command.Id))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(command.Id))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
