using FluentValidation.TestHelper;
using LogisticsManagement.Application.Common.Interfaces.IMiscTypeMaster;
using LogisticsManagement.Application.MiscTypeMaster.Commands.UpdateMiscTypeMaster;
using LogisticsManagement.Presentation.Validation.Common;
using LogisticsManagement.Presentation.Validation.MiscTypeMaster;
using LogisticsManagement.UnitTests.TestData;

namespace LogisticsManagement.UnitTests.Validators.MiscTypeMaster
{
    public sealed class UpdateMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateMiscTypeMasterCommandValidator CreateValidator()
        {
            var maxLengthProvider = new MaxLengthProvider(null!);
            return new UpdateMiscTypeMasterCommandValidator(maxLengthProvider, _mockQueryRepo.Object);
        }

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand();
            SetupAllAsyncMocks(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand(id: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NonExistentId_FailsValidation()
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand(id: 99);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyDescription_FailsValidation(string? description)
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand(description: description);
            SetupAllAsyncMocks(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public async Task Validate_InvalidIsActive_FailsValidation()
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand(isActive: 5);
            SetupAllAsyncMocks(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Fact]
        public async Task Validate_IsActiveZero_PassesValidation()
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand(isActive: 0);
            SetupAllAsyncMocks(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.IsActive);
        }

        [Fact]
        public async Task Validate_IsActiveOne_PassesValidation()
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand(isActive: 1);
            SetupAllAsyncMocks(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.IsActive);
        }
    }
}
